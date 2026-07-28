using AutoMapper;
using Library.ControllerApi.Mapping;
using Microsoft.Extensions.Logging.Abstractions;

namespace Library.Tests.Unit.Fixtures;

//Class fixture: xUnit builds this ONCE for the whole test class and hands it
//to each test via the constructor. Put expensive stateless setup here
// to save time on test running. MapperConfiguration scans profiles with reflection
// you don't wanna do that once PER TEST in a file with dozens of tests to run

public class MapperFixture
{
    
    public IMapper Mapper { get; }

    public MapperFixture()
    {
        // Same config as was in the InventoryControllerTests constructor
        var config = new MapperConfiguration(cfg => 
            cfg.AddProfile<MappingProfile>(), NullLoggerFactory.Instance);
        // Now used to create our Mapper property's object
        Mapper = config.CreateMapper();
    }

}