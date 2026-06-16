// This class will be our actual Library Catalog store of info
using Serilog; // bringing in that outside code we downloaded

namespace LibraryKata.Domain;

public class InMemoryLibraryRepository : ILibraryRepository
{

    // Because we don't have an outside store of info (like a SQL database)
    // we are kind of forced to rely on a list. We  will store info outside 
    // of program execution - I promise.
    private readonly List<LibraryItem> _items = new();

    public void Add(LibraryItem item)
    {
        _items.Add(item);

        // We just added a new item - thats a significant event. Lets log it.
        // Notice not using string interpolation - this uses Serilog's template
        // string format
        Log.Information("Added {Title} - id: {id}", item.Title, item.Id);
    }

    public List<LibraryItem> GetAll()
    {   
        // Don't want to accidentally pass a pointer to my real list 
        // return a new copy of the list
        return _items.ToList();
    }

    public LibraryItem GetById(int id)
    {
        
        // In order to find an item in our collection with the given Id
        // we need to search for it. We could use something like LINQ, 
        // but that's is own lesson/day
        foreach ( LibraryItem item in _items)
        {
            // Loop through the list, check for an item with the given Id
            // if we don't find it, throw that exception
            if( item.Id == id)
            {
                return item;
            }
        }

        // If we make it here - we exited the foreach without finding an item for that id
        Log.Warning("Lookup failed for id {Id}", id);
        throw new ItemNotFoundException(id); // throwing our custom exception, with offending id
    }

    public bool Remove(int id)
    {
        foreach (LibraryItem item in _items)
        {
            if(item.Id == id)
            {
                _items.Remove(item); // remove the item from the list
                Log.Information("Removed item with id {Id}", id); // log the removal
                return true;
            }
        }

        Log.Information("Removal failed for item with id {Id}", id);
        return false;
    }
}