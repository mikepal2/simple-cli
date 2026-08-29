using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Linq;
using System.Text.RegularExpressions;

namespace SnapCLI
{
    /// <summary>
    /// SnapCLI extensions
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Validates that only one of specified options appear on command line.
        /// </summary>
        /// <param name = "parseResult">Command line parse result.</param>
        /// <param name = "mutuallyExclusiveOptionsArguments">List of mutually exclusive options/arguments names. If there are multiple groups of mutually exclusive options/arguments, they must be enclosed in parentheses. Example: (option1,option2)(option3,arg1).</param>
        /// <param name = "commands">Optional list of commands this validation applies. If not specified, validation applies to all commands.</param>
        /// <exception cref = "CommandLineInputException">When there are mutually exclusive options found on command line.</exception>
        /// <exception cref = "ArgumentNullException">Option name or command name is <code>null</code>.</exception>
        public static void ValidateMutuallyExclusiveOptionsArguments(this ParseResult parseResult, string mutuallyExclusiveOptionsArguments, string[]? commands = null)
        {
            var groups = Regex.Split(mutuallyExclusiveOptionsArguments, @"(?<=[)])")
                .Select(x => x.Trim(CLI.NameListDelimiters))
                .Where(x => x != "")
                .ToArray();

            if (groups.Length == 0)
                return;

            foreach (var _group in groups)
            {
                var group = _group;
                if (group.StartsWith("("))
                {
                    if (!group.EndsWith(")"))
                        throw new AttributeUsageException("Invalid mutually exclusive options/arguments syntax (unmatched parentheses): " + mutuallyExclusiveOptionsArguments);
                    group = group.Substring(1, group.Length - 2);
                }
                if (group.Contains('(') || group.Contains(')'))
                    throw new AttributeUsageException("Invalid mutually exclusive options/arguments syntax (unmatched parentheses) " + mutuallyExclusiveOptionsArguments);

                var optionsList = group.Split(CLI.NameListDelimiters, StringSplitOptions.RemoveEmptyEntries);
                if (optionsList.Length == 0)
                    continue;

                ValidateMutuallyExclusiveOptionsArguments(parseResult, optionsList, commands);
            }
        }

        /// <summary>
        /// Validates that only one of specified options appear on command line.
        /// </summary>
        /// <param name = "parseResult">Command line parse result.</param>
        /// <param name = "mutuallyExclusiveOptionsArguments">List of mutually exclusive options/arguments names.</param>
        /// <param name = "commands">Optional list of commands this validation applies. If not specified, validation applies to all commands.</param>
        /// <exception cref = "CommandLineInputException">When there are mutually exclusive options found on command line.</exception>
        /// <exception cref = "AttributeUsageException">When a name does not match any option or argument reachable from the current command.</exception>
        /// <exception cref = "ArgumentNullException">Option name or command name is <code>null</code>.</exception>
        public static void ValidateMutuallyExclusiveOptionsArguments(this ParseResult parseResult, string[] mutuallyExclusiveOptionsArguments, string[]? commands = null)
        {
            if (mutuallyExclusiveOptionsArguments == null || mutuallyExclusiveOptionsArguments.Length == 0)
                return;

            if (mutuallyExclusiveOptionsArguments.Length < 2)
                throw new ArgumentException($"At least two options must be specified for {nameof(ValidateMutuallyExclusiveOptionsArguments)} method");

            if (commands != null)
            {
                if (!commands.Any(commandName => parseResult.RootCommandResult.Command.GetCommand(commandName ?? throw new ArgumentNullException(nameof(commandName))) == parseResult.CommandResult.Command))
                {
                    return;
                }
            }

            ValidateNamesAreKnown(parseResult, mutuallyExclusiveOptionsArguments);

            var commandLineArgs = parseResult.RootCommandResult.Children // cannot access option.IsGlobal(), assume all options of root command are global options
                    .Concat(parseResult.CommandResult.Children) // command options
                    .Select(r =>
                    {
                        if (r is ArgumentResult argResult
                            && argResult.Tokens.Count > 0
                            && mutuallyExclusiveOptionsArguments.Any(name => argResult.Argument.NameEquals(name ?? throw new ArgumentNullException(nameof(name)))))
                            return $"argument '{argResult.Argument.Name}'";
                        if (r is OptionResult optResult
                            && !optResult.Implicit
                            && mutuallyExclusiveOptionsArguments.Any(name => optResult.Option.NameEquals(name ?? throw new ArgumentNullException(nameof(name)))))
                            return $"option '{optResult.Option.Name}'";
                        return null;
                    })
                    .Where(x => x != null)
                    .Distinct()
                    .ToArray();

            if (commandLineArgs.Length > 1)
                throw new CommandLineInputException($"{commandLineArgs[0]} and {commandLineArgs[1]} are mutually exclusive for command '{parseResult.CommandResult.Command.FullName()}'");
        }

        // A misspelled name would otherwise make the whole check silently pass, so reject names that
        // match nothing on the current command or on any of its ancestors (recursive/global options).
        private static void ValidateNamesAreKnown(ParseResult parseResult, string[] names)
        {
            var known = new List<Symbol>();
            for (Command? command = parseResult.CommandResult.Command; command != null; command = command.Parents.OfType<Command>().FirstOrDefault())
            {
                known.AddRange(command.Options);
                known.AddRange(command.Arguments);
            }

            foreach (var name in names)
            {
                if (name == null)
                    throw new ArgumentNullException(nameof(names));
                if (!known.Any(symbol => symbol.NameEquals(name)))
                    throw new AttributeUsageException(
                        $"'{name}' listed as mutually exclusive is not an option or argument of command '{parseResult.CommandResult.Command.FullName()}'");
            }
        }

        /// <summary>
        /// Find option by specified name or <code>null</code> if command has no such option.
        /// </summary>
        /// <param name = "command"></param>
        /// <param name = "name">Option name or alias to find.</param>
        /// <returns>Option matching specified name or <code>null</code> if command has no such option.</returns>
        public static Option? GetOption(this Command command, string name)
        {
            return command.Options.FirstOrDefault(opt => opt.NameEquals(name));
        }

        /// <summary>
        /// Find command by name.
        /// </summary>
        /// <param name = "command">Root command to start search, usually RootCommand.</param>
        /// <param name = "name">Full name of command to find. Subcommands are separated by spaces.</param>
        /// <returns>Command matching the <paramref name = "name"/></returns>
        public static Command? GetCommand(this Command? command, string name)
        {
            foreach (var subcommandName in name.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (command == null)
                    break;
                command = command.Subcommands.FirstOrDefault(c => c.Name == subcommandName || c.Aliases.Any(alias => alias == subcommandName));
            }
            return command;
        }

        /// <summary>
        /// Return command full name including parents.
        /// </summary>
        /// <param name = "command"></param>
        /// <returns>Command full name including parents.</returns>
        public static string FullName(this Command command)
        {
            var name = command.Name;
            Symbol? parent = command;
            while (parent != null)
            {
                parent = parent.Parents.OfType<Command>().FirstOrDefault();
                if (parent is RootCommand)
                    break;
                if (parent != null)
                    name = parent.Name + " " + name;
            }
            return name;
        }

        /// <summary>
        /// Compares symbol name taking into account any aliases and full names.
        /// </summary>
        /// <param name = "symbol"><see cref = "Command"/>, <see cref = "Option"/> or <see cref = "Argument"/>.</param>
        /// <param name = "name">Name to match.</param>
        /// <returns>True if symbol has matching name or alias.</returns>
        public static bool NameEquals(this Symbol symbol, string name)
        {
            switch (symbol)
            {
                case Option opt:
                    name = name.TrimStart('-');
                    return opt.Name.TrimStart('-') == name || opt.Aliases.Any(alias => alias.TrimStart('-') == name);
                case Command command:
                    if (command.Aliases.Any(alias => alias == name))
                        return true;
                    return command.FullName() == name;
                default:
                    return symbol.Name == name;
            }
        }
    }
}
