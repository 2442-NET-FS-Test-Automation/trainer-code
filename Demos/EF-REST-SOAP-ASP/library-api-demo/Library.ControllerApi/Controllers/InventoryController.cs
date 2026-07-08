using AutoMapper;
using Library.ControllerApi.DTOs;
using Library.ControllerApi.Services;
using Library.Data;
using Library.Data.Entities;
using Microsoft.AspNetCore.Mvc; // ControllerBase lives here


[ApiController] // This annotation tells ASP.NET to map this controller during app.MapControllers()
[Route("api/[controller]")] // Pretty sure this will be localhost:5051/api/Inventory as the route base
public class InventoryController : ControllerBase
{   
    // This will be removed tomorrow for sure
    private readonly IInventoryService _service; 
    private readonly IMapper _mapper; // automapper object

    public InventoryController(IInventoryService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    // Lets write our first GET endpoint
    [HttpGet] // ActionResult just represents possible HTTP response actions
    public async Task<ActionResult<IEnumerable<InventoryDto>>> Get()
    {   
        var items = await _service.AllAsync();
        
        var mappedItems = _mapper.Map<List<InventoryDto>>(items);

        return Ok(mappedItems);
        
        // As is this creates an infinite loop when we try to serialize to JSON
        //return Ok(await _repo.GetAllAsync());

        // The fix is using a DTO - Data Transfer Object. In general it is bad practice
        // to send models as returns (or take them as arugments) to/from controller methods
        // // Models are for your API, not for the front end
        // var items = await _repo.GetAllAsync(); // get all items 

        // // This is what we will send back once we populate it
        // EntireInventoryDTO response = new();

        // // Now we need to map to those DTOs 
        // foreach(var item in items)
        // {
        //     // Creating an inventoryReturnDTO
        //     InventoryReturnDTO i = new InventoryReturnDTO
        //     {
        //         Name = item.Product.Name,
        //         Sku = item.Product.Sku,
        //         CurrentStock = item.CurrentStock
        //     }; 

        //     // To then populate the EntireInventoryDTO
        //     response.EntireInventory.Add(i);
        // }
        // // Returning the EntireInventoryDTO response
        // return Ok(response);

    }
    

    // localhost:5137/api/Inventory/{sku} - sku is passed in by the user
    // We can add routing info right on the annotation
    [HttpGet("{sku}")] // I can parameterize the route itself 
    public async Task<ActionResult<InventoryDto>> GetBySku(string sku)
    {

        var item = await _service.BySkuAsync(sku);

        if (item is null)
        {
            return NotFound(); // 404 not found
        }
        else
        {
            var mappedItem = _mapper.Map<InventoryDto>(item);
            return Ok(mappedItem);
        }


        // var item = await _repo.GetInventoryItemBySkuAsync(sku);
        
        // if(item is null)
        //     return NotFound();

        // var response = new InventoryReturnDTO
        // {
        //     Name = item.Product.Name,
        //     Sku = item.Product.Sku,
        //     CurrentStock = item.CurrentStock
        // };

        // // Then we check what to return based on item being null or not
        //  // returns a 404 - Sku didn't exist in db
        // return Ok(response); // 200 - found something - sent back to front end 

    }

    [HttpPost]
    public async Task<ActionResult<InventoryDto>> Create(InventoryCreateDto newInv)
    {
        var created = await _service.AddAsync(newInv);
        var response = _mapper.Map<InventoryDto>(created);

        // CreatedAt (201) works a little differently from our other response ActionResults
        // Created at needs to know how to find the newly created resource - so we tell it
        // Use the GetBySku controller method (literally the one above) and use the information
        // in response to build the URI string
        return CreatedAtAction(nameof(GetBySku), new { sku = response.Sku}, response);
    }

    [HttpDelete("{sku}")]
    public async Task<ActionResult> Delete(string sku)
    {
        bool isDeleted = await _service.RemoveAsync(sku);

        if (isDeleted)
        {
            return NoContent(); // 204 - No content - it WAS there, not anymore
        }
        else
        {
            return NotFound(); // 404 - couldn't delete it because your sku was wrong

            // Custom return message with StatusCode
            //return StatusCode(404, "Not found");
        }
    }

}
