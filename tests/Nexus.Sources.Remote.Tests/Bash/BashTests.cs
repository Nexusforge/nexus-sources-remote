// using Microsoft.Extensions.Logging;
// using Nexus.DataModel;
// using Nexus.Extensibility;
// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using System.Runtime.InteropServices;
// using System.Threading;
// using System.Threading.Tasks;
// using Xunit;
// using Xunit.Abstractions;

// namespace DataSource
// {
//     public class BashTests
//     {
//         private ILogger _logger;

//         public BashTests(ITestOutputHelper xunitLogger)
//         {
//             _logger = new XunitLoggerProvider(xunitLogger).CreateLogger(nameof(BashTests));
//         }

// #if LINUX
//         [Fact]
// #else
//         [Fact(Skip = "Linux-only test.")]
// #endif
//         public async Task ProvidesCatalog()
//         {
//             // arrange
//             var dataSource = new RemoteDataSource() as IDataSource;

//             var context = new DataSourceContext()
//             {
//                 ResourceLocator = new Uri(Path.Combine(Directory.GetCurrentDirectory(), "TESTDATA")),
//                 Configuration = new Dictionary<string, string>()
//                 {
//                     ["command"] = "Bash.sh",
//                     ["arguments"] = "55556",
//                     ["listen-address"] = "127.0.0.1",
//                     ["listen-port"] = "55556",
//                 },
//                 Logger = _logger,
//             };

//             await dataSource.SetContextAsync(context, CancellationToken.None);

//             // act
//             var actual = await dataSource.GetCatalogAsync("/A/B/C", CancellationToken.None);

//             // assert
//             var actualProperties1 = actual.Properties;
//             var actualIds = actual.Resources.Select(resource => resource.Id).ToList();
//             var actualUnits = actual.Resources.Select(resource => resource.Properties["Unit"]).ToList();
//             var actualGroups = actual.Resources.SelectMany(
//                 resource => resource.Properties.Where(current => current.Key.StartsWith("Groups"))).Select(current => current.Value).ToList();
//             var actualDataTypes = actual.Resources.SelectMany(resource => resource.Representations.Select(representation => representation.DataType)).ToList();

//             var expectedProperties1 = new Dictionary<string, string>() { ["a"] = "b" };
//             var expectedIds = new List<string>() { "resource1", "resource2" };
//             var expectedUnits = new List<string>() { "°C", "bar" };
//             var expectedGroups = new List<string>() { "group1", "group2" };
//             var expectedDataTypes = new List<NexusDataType>() { NexusDataType.INT64, NexusDataType.FLOAT64 };

//             Assert.True(actualProperties1.SequenceEqual(expectedProperties1));
//             Assert.True(expectedIds.SequenceEqual(actualIds));
//             Assert.True(expectedUnits.SequenceEqual(actualUnits));
//             Assert.True(expectedGroups.SequenceEqual(actualGroups));
//             Assert.True(expectedDataTypes.SequenceEqual(actualDataTypes));
//         }

// #if LINUX
//         [Fact]
// #else
//         [Fact(Skip = "Linux-only test.")]
// #endif
//         public async Task CanProvideTimeRange()
//         {
//             var expectedBegin = new DateTime(2019, 12, 31, 12, 00, 00, DateTimeKind.Utc);
//             var expectedEnd = new DateTime(2020, 01, 02, 09, 50, 00, DateTimeKind.Utc);

//             var dataSource = new RemoteDataSource() as IDataSource;

//             var context = new DataSourceContext()
//             {
//                 ResourceLocator = new Uri(Path.Combine(Directory.GetCurrentDirectory(), "TESTDATA")),
//                 Configuration = new Dictionary<string, string>()
//                 {
//                     ["command"] = "Bash.sh",
//                     ["arguments"] = "55556",
//                     ["listen-address"] = "127.0.0.1",
//                     ["listen-port"] = "55556",
//                 },
//                 Logger = _logger,
//             };

//             await dataSource.SetContextAsync(context, CancellationToken.None);

//             var actual = await dataSource.GetTimeRangeAsync("/A/B/C", CancellationToken.None);

//             Assert.Equal(expectedBegin, actual.Begin);
//             Assert.Equal(expectedEnd, actual.End);
//         }

// #if LINUX
//         [Fact]
// #else
//         [Fact(Skip = "Linux-only test.")]
// #endif
//         public async Task CanProvideAvailability()
//         {
//             var dataSource = new RemoteDataSource() as IDataSource;

//             var context = new DataSourceContext()
//             {
//                 ResourceLocator = new Uri(Path.Combine(Directory.GetCurrentDirectory(), "TESTDATA")),
//                 Configuration = new Dictionary<string, string>()
//                 {
//                     ["command"] = "Bash.sh",
//                     ["arguments"] = "55556",
//                     ["listen-address"] = "127.0.0.1",
//                     ["listen-port"] = "55556",
//                 },
//                 Logger = _logger,
//             };

//             await dataSource.SetContextAsync(context, CancellationToken.None);

//             var begin = new DateTime(2020, 01, 02, 00, 00, 00, DateTimeKind.Utc);
//             var end = new DateTime(2020, 01, 03, 00, 00, 00, DateTimeKind.Utc);
//             var actual = await dataSource.GetAvailabilityAsync("/A/B/C", begin, end, CancellationToken.None);

//             Assert.Equal(2 / 144.0, actual, precision: 4);
//         }

// #if LINUX
//         [Fact]
// #else
//         [Fact(Skip = "Linux-only test.")]
// #endif
//         public async Task CanReadFullDay()
//         {
//             var dataSource = new RemoteDataSource() as IDataSource;

//             var context = new DataSourceContext()
//             {
//                 ResourceLocator = new Uri(Path.Combine(Directory.GetCurrentDirectory(), "TESTDATA")),
//                 Configuration = new Dictionary<string, string>()
//                 {
//                     ["command"] = "Bash.sh",
//                     ["arguments"] = "55556",
//                     ["listen-address"] = "127.0.0.1",
//                     ["listen-port"] = "55556",
//                 },
//                 Logger = _logger,
//             };

//             await dataSource.SetContextAsync(context, CancellationToken.None);

//             var catalog = await dataSource.GetCatalogAsync("/A/B/C", CancellationToken.None);
//             var resource = catalog.Resources.First();
//             var representation = resource.Representations.First();
//             var catalogItem = new CatalogItem(catalog, resource, representation);

//             var begin = new DateTime(2019, 12, 31, 0, 0, 0, DateTimeKind.Utc);
//             var end = new DateTime(2020, 01, 03, 0, 0, 0, DateTimeKind.Utc);
//             var (data, status) = ExtensibilityUtilities.CreateBuffers(representation, begin, end);

//             var length = 3 * 86400;
//             var expectedData = new long[length];
//             var expectedStatus = new byte[length];

//             MemoryMarshal.AsBytes(expectedData.AsSpan()).Fill((byte)'d');
//             expectedStatus.AsSpan().Fill((byte)'s');

//             var request = new ReadRequest(catalogItem, data, status);
//             await dataSource.ReadAsync(begin, end, new ReadRequest[] { request }, new Progress<double>(), CancellationToken.None);
//             var longData = data.Cast<byte, long>();

//             Assert.True(expectedData.SequenceEqual(longData.ToArray()));
//             Assert.True(expectedStatus.SequenceEqual(status.ToArray()));
//         }
//     }
// }