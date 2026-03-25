using Tm7.Cli;

var rootCommand = CommandFactory.CreateRootCommand();
return await rootCommand.Parse(args).InvokeAsync();
