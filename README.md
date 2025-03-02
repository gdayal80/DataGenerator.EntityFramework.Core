
![Nuget](https://img.shields.io/nuget/v/DataGenerator.OpenAI)

# Introduction
DataGenerator.OpenAI is a package to generate Mock Data using OpenAI & EntityFramework Core on Windows / Linux / MacOS. The package is compatible only with primitive DataTypes, Complex DataTypes are not supported.

# Limitations
Due to Current limitation of OpenAI supported data types for structured data, this package supports only following data types

`Boolean` `Number` `String` `Int32` `Int64` `DateTime`

though `DateTine` is not supported by OpenAI, this package assigns current dateTime as default value to DateTime data type

# Parameter: `openAiBatchSize` 

`openAiBatchSize` should be equal to `noOfRows` in case you want to ensure unique data values.

# Parameter: `inDataValue` 

`inDataValue` is the value of the data under which you want to generate data. For instance under state `Maharashtra` you want to generate data for table `City` then pass `inDataValue` as `maharashtra`.

# Getting started

From nuget packages

![Nuget](https://img.shields.io/nuget/v/DataGenerator.OpenAI)

`PM> Install-Package DataGenerator.OpenAI`

## Usage 

```C#
using DataGenerator.OpenAI.Interfaces;
    
public class ConsoleTraceWriter : ITraceWriter
{
    public void Log(string message)
    {
        Console.WriteLine(message);
    }

    public void Verbose(string message)
    {
        Console.WriteLine(message);
    }
}
```
```C#
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Collections.Generic;
using DataGenerator.OpenAI.Mock.Data.Generators;
using DataGenerator.OpenAI.Data.Generators;
using System.Globalization;

Random random = new Random();
var connStr = Environment.GetEnvironmentVariable("LOCALHOST_MYSQL")!;
var dbOptions = new DbContextOptionsBuilder<Context>().UseMySql(connStr, ServerVersion.AutoDetect(connStr),
                    mySqlOptionsAction: (MySqlDbContextOptionsBuilder sqlOptions) =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 10,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                        sqlOptions.CommandTimeout(240);
                    }).ConfigureWarnings(w => w.Throw(RelationalEventId.MultipleCollectionIncludeWarning)).Options;
var context = new Context(dbOptions);
var trace = new ConsoleTraceWriter();
var openAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")!;
string locale = CultureInfo.CurrentCulture.Name;

try
{
    MockDataGenerator mockDataGenerator = new MockDataGenerator(trace, openAiApiKey);
    EntityFrameworkDataGenerator<Context> entityFrameworkDataGenerator = new EntityFrameworkDataGenerator<Context>(context, mockDataGenerator, trace);

    var users = await entityFrameworkDataGenerator.GenerateData<User>(locale, 5, 5);

    context.Users.AddRange(users!);

    var schools = await entityFrameworkDataGenerator.GenerateData<School>(locale, 1, 1);

    foreach (var school in schools!)
    {
        school.CreatedBy = users?[random.Next(0, users.Count() - 1)];
        school.UpdatedBy = users?[random.Next(0, users.Count() - 1)];
    }

    context.Schools.AddRange(schools!);

    await context.SaveChangesAsync();
}
catch (Exception ex)
{
    trace.Log(ex.Message);
}
```

# Third Parties
* [Microsoft.EntityFrameworkCore](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore/8.0.12)
* [OpenAI](https://www.nuget.org/packages/OpenAI/2.1.0)
