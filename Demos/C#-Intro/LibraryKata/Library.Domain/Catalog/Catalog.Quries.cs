using System.Collections;

namespace LibraryKata.Domain;

// The second half of my class
// I don't have to mirror the interface implementation or any inheritance across both class files
// however, I can still only inherit from parent
public partial class Catalog : IEnumerable<LibraryItem>
{
    // this is the one that we actually want to provide logic for, the one that uses a generic
    public IEnumerator<LibraryItem> GetEnumerator()
    {
        foreach ( LibraryItem item in _items )
        {
            // We want to lazily return items one at a time, we don't want to return a second list
            // or anything like that. We will use "yield" with out return 
            yield return item;
        }
    }

    // This version (non-generic version) is OLD - kept in IEnumerable for backwards compatibility reasons.
    // What we are doing is simply routing it to the IEnumerator<LibraryItem> GetEnumerator() method
    IEnumerator IEnumerable.GetEnumerator()
    {   
        // returns a call to IEnumerator<LibraryItem> GetEnumerator()
        return GetEnumerator();
    }

    // Lets make a method to return only lendable items (things that implement ILendable)
    public IEnumerable<LibraryItem> Lendable()
    {   

        foreach (LibraryItem item in _items )
        {   
            // Checking for type via "is"
            if( item is ILendable)
            {
                yield return item;
            }
        }
    }

    // Search function for the catalog 
    // We are going to use Predicate to pass a delegate to our function
    // A delegate is just a reference to method in an argument list
    // Predicate<LibraryItem> match represents a function that takes a LibraryItem, and returns a boolean

    // When we call this Find() method, we will combine it with a Lambda. Lambda's are the C# implementation
    // of anonymous or arrow functions. Just a quick definition that we don't bother storing a reference to.
    // authorItems = Find(item => item.Author == "Frank Herbert"); - "find every item where it's author equals "Frank Herbert" 
    public List<LibraryItem> Find(Predicate<LibraryItem> match)
    {
        // match is a method, not an object or a value
        // its a pointer to some method that gets passed in when we call Find()
        List<LibraryItem> foundItems = new();

        foreach (LibraryItem item in _items)
        {
            if(match(item))
            {
                foundItems.Add(item);
            }
        }

        return foundItems;
    }

}