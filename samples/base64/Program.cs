using SnapCLI;

// Simple multi-command CLI example.
//
// In this example, the two methods Encode() and Decode() are declared with the [Command] attribute and represent
// two entry points for the CLI application, each associated with the corresponding command. The command names are
// automatically derived from the method names as 'encode' and 'decode', respectively.

class Program
{
    [Command]
    public static void Encode([Argument] string text)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(text);
        var base64 = System.Convert.ToBase64String(bytes);
        Console.WriteLine(base64);
    }

    [Command]
    public static void Decode([Argument] string base64)
    {
        var bytes = System.Convert.FromBase64String(base64);
        var text = System.Text.Encoding.UTF8.GetString(bytes);
        Console.WriteLine(text);
    }
}

/* This program generates following output
   
> base64 -?
Description:

Usage:
  base64 [command] [options]

Options:
  -?, -h, --help  Show help and usage information
  --version       Show version information

Commands:
  encode <text>
  decode <base64>

> base64 encode "Hello World!"
SGVsbG8gV29ybGQh

> base64 decode SGVsbG8gV29ybGQh
Hello World!

 */


