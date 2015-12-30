﻿#region Using Statements
    using System;
    using System.Collections.Generic;

    using Cake.Host;
    using Cake.Host.Commands;
    using Cake.Core;
    using Cake.Host.Arguments;
    using Cake.Core.Diagnostics;
    using Cake.Host.Diagnostics;
#endregion



namespace CakeBoss.Host
{
    /// <summary>
    /// The Cake application.
    /// </summary>
    public sealed class HostApplication
    {
        #region Fields (4)
            private readonly IVerbosityAwareLog _log;
            private readonly ICommandFactory _commandFactory;
            private readonly IArgumentParser _argumentParser;
            private readonly IConsole _console;
        #endregion





        #region Constructor (1)
            /// <summary>
            /// Initializes a new instance of the <see cref="CakeApplication"/> class.
            /// </summary>
            /// <param name="log">The log.</param>
            /// <param name="commandFactory">The command factory.</param>
            /// <param name="argumentParser">The argument parser.</param>
            /// <param name="console">The console.</param>
            public HostApplication(
                IVerbosityAwareLog log,
                ICommandFactory commandFactory,
                IArgumentParser argumentParser,
                IConsole console)
            {
                if (log == null)
                {
                    throw new ArgumentNullException("log");
                }
                if (commandFactory == null)
                {
                    throw new ArgumentNullException("commandFactory");
                }
                if (argumentParser == null)
                {
                    throw new ArgumentNullException("argumentParser");
                }
                if (console == null)
                {
                    throw new ArgumentNullException("console");
                }

                _log = log;
                _commandFactory = commandFactory;
                _argumentParser = argumentParser;
                _console = console;
            }
        #endregion





        #region Functions (2)
            /// <summary>
            /// Runs the application with the specified arguments.
            /// </summary>
            /// <param name="args">The arguments.</param>
            /// <returns>The application exit code.</returns>
            public bool Run(IEnumerable<string> args)
            {
                try
                {
                    // Parse options.
                    var options = _argumentParser.Parse(args);
                    if (options != null)
                    {
                        _log.SetVerbosity(options.Verbosity);
                    }

                    // Create the correct command and execute it.
                    var command = this.CreateCommand(options);
                    return command.Execute(options);
                }
                catch (Exception ex)
                {
                    if (_log.Verbosity == Verbosity.Diagnostic)
                    {
                        _log.Error("Error: {0}", ex);
                    }
                    else
                    {
                        _log.Error("Error: {0}", ex.Message);
                    }

                    return false;
                }
            }

            public ICommand CreateCommand(CakeOptions options)
            {
                if (options != null)
                {
                    if (options.ShowHelp)
                    {
                        return _commandFactory.CreateHelpCommand();
                    }

                    if (options.ShowVersion)
                    {
                        return _commandFactory.CreateVersionCommand();
                    }

                    if (options.Script != null)
                    {
                        if (options.PerformDryRun)
                        {
                            return _commandFactory.CreateDryRunCommand();
                        }

                        if (options.ShowDescription)
                        {
                            _log.SetVerbosity(options.Verbosity);
                            return _commandFactory.CreateDescriptionCommand();
                        }

                        return _commandFactory.CreateBuildCommand();
                    }
                }

                _console.WriteLine();
                _log.Error("Could not find a build script to execute.");
                _log.Error("Either the first argument must the build script's path,");
                _log.Error("or build script should follow default script name conventions.");

                return new ErrorCommandDecorator(_commandFactory.CreateHelpCommand());
            }
        #endregion
    }
}
