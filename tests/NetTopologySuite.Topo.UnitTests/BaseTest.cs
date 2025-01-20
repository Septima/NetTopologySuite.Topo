using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.IO.Converters;

namespace NetTopologySuite.Topo.UnitTests;

public class BaseTest
{
    private readonly JsonSerializerOptions jsonSerializerOptions;
    private readonly WKTReader reader = new();
    private readonly WKTWriter writer = new();

    public ServiceProvider ServiceProvider { get; set; } = null!;
    public TopologyEditor TopologyEditor { get; set; } = null!;
    public Topology Topology { get; set; } = null!;

    public BaseTest()
    {
        jsonSerializerOptions = new JsonSerializerOptions();
        jsonSerializerOptions.Converters.Add(new GeoJsonConverterFactory());
    }

    [TestInitialize()]
    public void Initialize()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        ServiceProvider = new ServiceCollection()
            .AddLogging(builder =>
            {
                builder.AddConfiguration(configuration.GetSection("Logging"));
                builder.AddConsole();
            })
            .AddTransient<TopoFactory>()
            .AddTransient<TopologyEditor>()
            .BuildServiceProvider() ?? throw new Exception("Cannot initialize");
        TopologyEditor = ServiceProvider.GetRequiredService<TopologyEditor>();
        jsonSerializerOptions.Converters.Add(new GeoJsonConverterFactory());
        Topology = new Topology();
    }

    public T Read<T>(string wkt) where T : class => (reader.Read(wkt) as T)!;

    public string Write(Geometry geometry) => writer.Write(geometry);

    public void CreateTopologyFromFile(string filename)
    {
        var assemblyPath = Assembly.GetExecutingAssembly().Location;
        var projectDirectory = Directory.GetParent(assemblyPath)?.Parent?.Parent?.Parent?.FullName;
        var geojson = File.ReadAllText(Path.Join(projectDirectory, "data", filename + ".geojson"));
        var fc = JsonSerializer.Deserialize<FeatureCollection>(geojson, jsonSerializerOptions);
        var lineStrings = fc?.Select(f => f.Geometry as LineString);
        foreach (var lineString in lineStrings!)
            Topology = TopologyEditor.AddLineString(Topology, lineString!);
    }


    public void AssertAreNormalizedEqual(Geometry expected, Geometry actual)
    {
        if(!expected.EqualsNormalized(actual))
            throw new AssertFailedException($"Expected {Write(expected)} but was {Write(actual)}");
    }
}