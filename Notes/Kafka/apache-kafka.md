# Apache Kafka: The Distributed Commit Log and Its Event Streams

## Learning Objectives
- Describe Kafka as a distributed, append-only commit log rather than a traditional message queue.
- Define the core vocabulary: topic, partition, offset, producer, consumer, consumer group, broker, and replication.
- Explain what an "event stream" is — a retained, ordered, replayable sequence — and how consumers track their own position with offsets.
- Contrast Kafka with a traditional queue along the three properties that matter most: retention, replay, and multiple independent consumer groups.
- Read a small producer/consumer sketch and map it back to the concepts.

## Why This Matters
When people say "event streaming platform," Apache Kafka is almost always the concrete system they have in mind. It is the de-facto backbone for high-throughput pipelines: moving events between microservices, feeding real-time analytics, powering activity tracking, and acting as the durable spine of an event-driven estate. Understanding Kafka is really about understanding one deceptively simple idea — **the commit log** — and how a handful of concepts (partitions, offsets, consumer groups) fall out of it. Interviewers reach for Kafka precisely because that idea separates candidates who have memorized buzzwords from those who understand why Kafka behaves so differently from a message queue. Managed equivalents like Azure Event Hubs (which even speaks the Kafka protocol) and Confluent Cloud make the model even more common in production, so the concepts here travel well beyond one product.

## The Concept

### The core idea: a distributed commit log
The mental model to hold onto is not "queue." It is **log** — an append-only file where new records are added at the end and each record gets a sequential number. This is the same primitive that databases use internally for their write-ahead/commit logs. Kafka takes that primitive, makes it the *public interface*, and distributes it across a cluster of machines.

```
A log is an ordered, append-only sequence. New events go on the right.
Each event has a stable position (offset) that never changes.

  offset:   0     1     2     3     4     5     6   -> (append here)
          +-----+-----+-----+-----+-----+-----+-----+
  events  | e0  | e1  | e2  | e3  | e4  | e5  | e6  |
          +-----+-----+-----+-----+-----+-----+-----+
```

Two properties make the log special compared to a queue. First, **reading does not remove anything.** A consumer reading offset 3 does not delete offset 3; the event stays in the log for everyone else and for later replay. Second, **each consumer remembers its own position.** The log is passive; consumers pull, and each one tracks how far it has read. These two properties are the source of nearly every difference between Kafka and a traditional broker.

### Topics and partitions
A **topic** is a named log — the category you publish to and subscribe from, for example `orders` or `page-views`. Conceptually a topic is one continuous stream, but for scale Kafka splits each topic into **partitions**.

A partition is the actual append-only log; a topic is a set of partitions. Partitioning is what lets Kafka scale horizontally and is the unit of parallelism and of ordering:

- **Ordering is guaranteed within a partition, not across a topic.** Events in partition 0 are strictly ordered by offset; there is no global order across partitions. This is the single most important caveat in Kafka.
- **A message's key decides its partition.** Producers can attach a key; Kafka hashes it so all events with the same key land in the same partition. Keying by `orderId` guarantees that every event for a given order is in one partition and therefore strictly ordered — while different orders spread across partitions for parallelism. No key means round-robin/sticky distribution and no per-entity ordering guarantee.

```
Topic "orders" split into 3 partitions:

  P0:  | o0 | o3 | o6 | o9 |        Each partition is its own ordered
  P1:  | o1 | o4 | o7 |             log. Order is guaranteed WITHIN a
  P2:  | o2 | o5 | o8 | o10 |       partition, never ACROSS the topic.
```

### Offsets
An **offset** is the sequential id of a record within a partition — a monotonically increasing integer starting at 0. Offsets are per-partition (partition 0 and partition 1 both have an offset 5, and they are unrelated records). The offset is how a consumer answers "where am I?": the consumer commits the offset of the last record it has successfully processed, so if it restarts it resumes from the next one. Because the consumer — not the broker — owns this position, a consumer can also deliberately **rewind** to an older offset to reprocess history, or jump to the latest to skip a backlog.

### Producers
A **producer** is a client that appends events to a topic. It chooses the topic and, optionally, a key (which fixes the partition). Producers are typically asynchronous and batched for throughput, and they choose a durability level via acknowledgements: `acks=0` (fire and forget), `acks=1` (leader confirmed), or `acks=all` (all in-sync replicas confirmed — the safest). Producers do not know or care who will consume the events; that decoupling is inherited straight from the event-driven model.

### Consumers and consumer groups
A **consumer** reads events from one or more partitions and processes them. The concept that ties Kafka together is the **consumer group**: a set of consumer instances that cooperate to read a topic, identified by a shared `group.id`.

Within a group, **each partition is assigned to exactly one consumer.** So a group divides the work of a topic across its members — this is how you scale out processing. If a topic has 4 partitions and the group has 4 consumers, each reads one partition in parallel. Add a fifth consumer and it sits idle (partitions cap the parallelism); drop to 2 consumers and each reads 2 partitions. Kafka **rebalances** assignments automatically as members join or leave.

The crucial second half: **different consumer groups are completely independent.** Each group has its own set of committed offsets and reads the *entire* topic on its own. So an `analytics` group and a `fraud-check` group both receive every event in `orders`, each at its own pace, neither affecting the other. This is how Kafka delivers pub/sub broadcast (across groups) and point-to-point work-sharing (within a group) from the same log.

```
Topic "orders" (4 partitions) read by TWO independent groups:

  Group "analytics" (2 consumers)      Group "fraud-check" (4 consumers)
    A1 <- P0, P1                          F1 <- P0
    A2 <- P2, P3                          F2 <- P1
                                          F3 <- P2
  Each group sees EVERY event and         F4 <- P3
  keeps its OWN offsets. Within a
  group, each partition goes to one consumer.
```

### Brokers, the cluster, and replication (high level)
A **broker** is a single Kafka server. A **cluster** is several brokers working together, and a topic's partitions are spread across them so the load and storage are distributed. (Coordination is handled by KRaft — Kafka's built-in Raft-based metadata quorum — in modern versions, replacing the older ZooKeeper dependency; you rarely need more than that fact.)

**Replication** is how Kafka survives a broker dying. Each partition is replicated to a configurable number of brokers (the **replication factor**, commonly 3). One replica is the **leader** — it handles all reads and writes for that partition — and the others are **followers** that copy the leader's log and stay in sync (the "in-sync replicas," ISR). If the leader's broker fails, one of the in-sync followers is promoted to leader and the partition keeps serving with no data loss. This is what lets Kafka claim durability and high availability: an event acknowledged with `acks=all` exists on multiple machines before the producer is told it succeeded.

### Event streams: retention, replay, and self-tracked offsets
Putting it together: **an event stream is the partitioned log viewed as a durable, ordered, replayable sequence of events.** Three properties define it.

- **Retention.** Kafka keeps events for a configured policy, not until they are consumed. That policy is time-based (e.g., keep 7 days), size-based (e.g., keep 50 GB per partition), or **compaction** (keep at least the latest event per key, so the log becomes a snapshot of current state per key). Events are removed by the retention policy, never by the act of reading them.
- **Replay.** Because events persist and consumers address them by offset, any consumer can re-read from any point. A new service can be pointed at offset 0 and rebuild its entire state from history; a buggy consumer can be fixed and rewound to reprocess. The log is a source of truth, not a transient courier.
- **Self-tracked offsets.** Each consumer group stores its own committed offset per partition (in an internal Kafka topic). The broker doesn't decide when a consumer is "done" — the consumer does, by committing offsets after it has processed. This is why a slow consumer never blocks a fast one and why replay is just a matter of choosing a different starting offset.

### Kafka versus a traditional queue
The contrast comes down to three things, all consequences of "log, not queue":

| | Traditional queue (e.g., RabbitMQ classic) | Apache Kafka (log) |
|---|---|---|
| **After a message is read** | Acknowledged, then **deleted** from the queue | **Retained** by policy; reading never deletes |
| **Replay of past messages** | Not possible — once consumed it's gone | Any consumer can rewind to an old offset and re-read |
| **Fan-out to many independent readers** | Needs separate queues / exchange bindings; each consumer competes or you duplicate the message | Every **consumer group** reads the whole topic independently from one log |
| **Ordering** | Generally FIFO per queue | Guaranteed **per partition** (use keys to control grouping) |
| **Scaling reads** | Add competing consumers on the queue | Add consumers up to the **partition count** within a group |

A traditional broker is optimized for transient task delivery — push a job, one worker does it, it's gone. Kafka is optimized for a durable, replayable stream that many independent consumers can read now and re-read later. Neither is strictly "better"; they solve different problems. Reach for a queue when you want a work list that empties; reach for Kafka when the sequence of events is itself valuable and multiple consumers — present and future — need it.

### A small producer/consumer sketch
Illustrative pseudocode in the style of the .NET `Confluent.Kafka` client. The point is the shape, not the exact API.

```csharp
// --- Producer: append an event to the "orders" topic ---
var producerConfig = new ProducerConfig {
    BootstrapServers = "broker1:9092,broker2:9092",  // any broker bootstraps the cluster
    Acks = Acks.All                                   // wait for all in-sync replicas
};
using var producer = new ProducerBuilder<string, string>(producerConfig).Build();

// Key = orderId, so all events for one order land in the same partition (ordered).
await producer.ProduceAsync("orders",
    new Message<string, string> { Key = "ORD-4471", Value = orderJson });

// --- Consumer: read the "orders" topic as part of a group ---
var consumerConfig = new ConsumerConfig {
    BootstrapServers = "broker1:9092",
    GroupId = "inventory-service",     // the consumer group; its own offsets
    AutoOffsetReset = AutoOffsetReset.Earliest, // where a brand-new group starts
    EnableAutoCommit = false           // commit offsets manually, after processing
};
using var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
consumer.Subscribe("orders");

while (running) {
    var result = consumer.Consume(cancellationToken);   // pull the next event
    Handle(result.Message.Value);                        // idempotent processing
    consumer.Commit(result);   // commit AFTER success -> at-least-once delivery
}
```

Two beats to notice. The consumer commits its offset **after** processing, so a crash mid-processing means the event is redelivered on restart — **at-least-once** delivery, which is why `Handle` must be idempotent. And a second service with a different `GroupId` reading the same `orders` topic would receive every one of these events independently, without either group affecting the other.

## Say It in an Interview
- *"Kafka is a distributed, append-only commit log, not a queue. A topic is a named log split into partitions; each event has an offset — its position in a partition. Reading doesn't delete anything, and each consumer tracks its own offset."*
- *"Ordering is only guaranteed within a partition, so you key messages — for example by order id — to force all events for one entity into the same partition while different entities spread out for parallelism."*
- *"A consumer group shares a topic's partitions among its members to scale processing, but different groups each read the whole topic independently with their own offsets — that's how Kafka gives you both work-sharing and broadcast from one log."*
- *"Versus a traditional queue, the three differences are retention, replay, and multiple independent consumer groups: Kafka keeps events by policy instead of deleting on read, lets any consumer rewind to an old offset, and lets many groups consume the same stream without competing."*

## Check Yourself
1. Why is "log" a better mental model for Kafka than "queue"? Name the two log properties that drive most of Kafka's behavior.
2. What is the relationship between a topic and a partition, and what ordering guarantee does a partition give that a topic does not?
3. You need every event for a given customer processed in order, but still want parallelism across customers. How do you arrange that in Kafka?
4. Within one consumer group of 3 consumers on a 6-partition topic, how is the work divided? What happens if you add a 7th consumer to that group?
5. Two teams both need to consume the `orders` topic without interfering with each other. What Kafka concept makes that clean, and how are their read positions kept separate?
6. Give the three properties along which Kafka differs from a traditional queue, and one consequence of each.
7. Reading events never deletes them — so what eventually removes them from a partition?

**Answers:** (1) Kafka exposes an append-only log as its interface; reads don't remove records and each consumer tracks its own position — those two properties explain retention, replay, and independent consumers. (2) A topic is a set of partitions; each partition is an ordered append-only log, so ordering is guaranteed within a partition but there is no global ordering across a topic. (3) Use the customer id as the message key so all of that customer's events hash to one partition (ordered), while different customers' keys spread across partitions for parallelism. (4) Each partition goes to exactly one consumer, so the 3 consumers get 2 partitions each; a 7th consumer on a 6-partition topic sits idle because partitions cap parallelism within a group. (5) Give each team its own consumer group — every group reads the entire topic independently and stores its own committed offsets per partition, so their positions and pace don't affect each other. (6) Retention (Kafka keeps events by policy, so nothing is lost on read), replay (any consumer can rewind to an old offset and reprocess), and multiple independent consumer groups (many readers consume the whole stream without competing). (7) The retention policy — time-based, size-based, or log compaction — not the act of consuming.

## Summary
- Kafka is a **distributed, append-only commit log**: reading never deletes, and each consumer tracks its own **offset**.
- A **topic** is a named log split into **partitions**; **offsets** number records within a partition, and **ordering is guaranteed only within a partition** — use a **key** to group an entity's events into one partition.
- **Producers** append events; **consumers** in a **consumer group** split the topic's partitions (one partition per consumer), while **different groups** each read the whole topic independently with their own offsets.
- **Brokers** form a cluster; **replication** (leader + in-sync followers) keeps partitions available and durable when a broker fails.
- An **event stream** is the log seen as a retained, ordered, replayable sequence; retention (time/size/compaction), replay by offset, and self-tracked offsets are its defining traits.
- Versus a traditional queue, Kafka wins on **retention, replay, and multiple independent consumer groups**; most brokers deliver **at-least-once**, so consumers must be **idempotent**.

## Resources
- [Apache Kafka Documentation — Introduction and Design](https://kafka.apache.org/documentation/)
- [Kafka 101 / Apache Kafka Fundamentals (Confluent Developer)](https://developer.confluent.io/courses/apache-kafka/events/)
- [What is Apache Kafka? — Azure Event Hubs for Apache Kafka (Microsoft Learn)](https://learn.microsoft.com/en-us/azure/event-hubs/azure-event-hubs-kafka-overview)
