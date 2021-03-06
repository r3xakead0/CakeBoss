﻿#region Using Statements
    using System;
    using System.Diagnostics;
    using System.Collections.Generic;

    using Cake.Core;
    using Cake.Core.Diagnostics;
    using Cake.Host.Diagnostics;
    using Cake.Host.Diagnostics.Formatting;

    using Serilog;
    using Serilog.Events;
#endregion



namespace CakeBoss.Host
{
    public sealed class HostLog : IVerbosityAwareLog
    {
        #region Fields (3)
            private readonly IConsole _console;
            private readonly object _lock;
            private readonly IDictionary<LogLevel, ConsolePalette> _palettes;
        #endregion





        #region Constructor (1)
            public HostLog(IConsole console)
            {
                _console = console;
                _lock = new object();
                _palettes = CreatePalette();
                Verbosity = Debugger.IsAttached ? Verbosity.Diagnostic : Verbosity.Normal;
            }
        #endregion





        #region Properties (1)
            public Verbosity Verbosity { get; private set; }
        #endregion





        #region Functions (1)
            public void Write(Verbosity verbosity, LogLevel level, string format, params object[] args)
            {
                if (verbosity > Verbosity)
                {
                    return;
                }
                lock (_lock)
                {
                    try
                    {
                        var palette = _palettes[level];
                        var tokens = FormatParser.Parse(format);
                        foreach (var token in tokens)
                        {
                            SetPalette(token, palette);
                            _console.Write("{0}", token.Render(args));
                        }

                        Log.Logger.Write(this.GetLogLevel(level), format, args);
                    }
                    finally
                    {
                        _console.ResetColor();
                        _console.WriteLine();
                    }
                }
            }

            public void SetVerbosity(Verbosity verbosity)
            {
                Verbosity = verbosity;
            }

            private void SetPalette(FormatToken token, ConsolePalette palette)
            {
                var property = token as PropertyToken;
                if (property != null)
                {
                    _console.BackgroundColor = palette.ArgumentBackground;
                    _console.ForegroundColor = palette.ArgumentForeground;
                }
                else
                {
                    _console.BackgroundColor = palette.Background;
                    _console.ForegroundColor = palette.Foreground;
                }
            }



            private IDictionary<LogLevel, ConsolePalette> CreatePalette()
            {
                var background = _console.BackgroundColor;
                var palette = new Dictionary<LogLevel, ConsolePalette>
                {
                    { LogLevel.Error, new ConsolePalette(ConsoleColor.DarkRed, ConsoleColor.White, ConsoleColor.Red, ConsoleColor.White) },
                    { LogLevel.Warning, new ConsolePalette(background, ConsoleColor.Yellow, background, ConsoleColor.Yellow) },
                    { LogLevel.Information, new ConsolePalette(background, ConsoleColor.White, ConsoleColor.DarkBlue, ConsoleColor.White) },
                    { LogLevel.Verbose, new ConsolePalette(background, ConsoleColor.Gray, background, ConsoleColor.White) },
                    { LogLevel.Debug, new ConsolePalette(background, ConsoleColor.DarkGray, background, ConsoleColor.Gray) }
                };
                return palette;
            }

            private LogEventLevel GetLogLevel(LogLevel level)
            {
                switch (level)
                {
                    case LogLevel.Fatal:
                        return LogEventLevel.Fatal;

                    case LogLevel.Error:
                        return LogEventLevel.Error;

                    case LogLevel.Warning:
                        return LogEventLevel.Warning;

                    case LogLevel.Information:
                        return LogEventLevel.Information;

                    case LogLevel.Verbose:
                        return LogEventLevel.Verbose;
                }

                return LogEventLevel.Debug;
            }
        #endregion
    }
}
