
![Nuget](https://img.shields.io/nuget/v/MockDataGenerator.EntityFramework.Core)

# Introduction
MockDataGenerator.EntityFramework.Core is a package to generate Mock Data using OpenAI & EntityFramework Core on Windows / Linux / MacOS (Though not tested on MacOS (due to unavailability of Mac Machine), but it might work).

# Getting started

From nuget packages

![Nuget](https://img.shields.io/nuget/v/MockDataGenerator.EntityFramework.Core)

`PM> Install-Package MockDataGenerator.EntityFramework.Core`

## Usage

### From html text 

```C#
using WeasyPrint.Wrapper;
using System.IO;

public class ConsoleTraceWriter : ITraceWriter
{
    public void Info(string message)
    {
        Console.WriteLine(message);
    }

    public void Verbose(string message)
    {
        
    }
}

string workingDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
var trace = new ConsoleTraceWriter();

using (WeasyPrintClient client = new WeasyPrintClient(trace, workingDir))
{
    var html = "<!DOCTYPE html><html><body><h1>Hello World</h1></body></html>";

    var binaryPdf = await client.GeneratePdfAsync(html);

    File.WriteAllBytes("result.pdf",binaryPdf);
}
```

### From html file
```C#
using (WeasyPrintClient client = new WeasyPrintClient(trace, workingDir))
{
    var input = @"path\to\input.html";
    var output = @"path\to\output.pdf";

    await client.GeneratePdfAsync(input, output);
}
```

### Watch output and errors
```C#
using (WeasyPrintClient client = new WeasyPrintClient(trace, workingDir))
{
    var input = @"path\to\input.html";
    var output = @"path\to\output.pdf";

    client.OnDataError += OnDataError;
    
    await client.GeneratePdfAsync(input, output);
}

private void OnDataError(OutputEventArgs e)
{
    Console.WriteLine(e.Data);
}
```

# Third Parties
* [WeasyPrint](https://weasyprint.org/) - BSD 3-Clause License
