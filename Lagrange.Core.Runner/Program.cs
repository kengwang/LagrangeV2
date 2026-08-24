using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Message;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Runner;

internal static class Program
{
    private static async Task Main()
    {
        
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;
        
        BotContext context;

        if (File.Exists("keystore.json"))
        {
            context = BotFactory.Create(new BotConfig
            {
                Protocol = Protocols.AndroidPad,
                LogLevel = LogLevel.Trace
            }, JsonSerializer.Deserialize<BotKeystore>(await File.ReadAllTextAsync("keystore.json")) ?? throw new InvalidOperationException());
        }
        else
        {
            context = BotFactory.Create(new BotConfig
            {
                Protocol = Protocols.AndroidPad,
                LogLevel = LogLevel.Trace
            });
        }
        
        AppDomain.CurrentDomain.ProcessExit += async (_, _) =>
        {
            await context.Logout();
        };
        
        context.EventInvoker.RegisterEvent<BotLogEvent>((_, args) =>
        {
            Console.WriteLine(args);
        });
        
        context.EventInvoker.RegisterEvent<BotQrCodeEvent>((_, args) =>
        {
            Console.WriteLine(args);
            QrCodeHelper.Output(args.Url, false);
        });
        
        context.EventInvoker.RegisterEvent<BotRefreshKeystoreEvent>(async (_, args) =>
        {
            await File.WriteAllTextAsync("keystore.json", JsonSerializer.Serialize(args.Keystore));
        });
        
        context.EventInvoker.RegisterEvent<BotCaptchaEvent>((_, args) =>
        {
            Console.WriteLine(args.CaptchaUrl);
            string token = Console.ReadLine()!;
            context.SubmitCaptcha(token, "");
        });
        
        context.EventInvoker.RegisterEvent<BotSMSEvent>((_, args) =>
        {
            Console.WriteLine(args.Phone);
            Console.WriteLine(args.Url);
            var sms = Console.ReadLine();
            context.SubmitSMSCode(sms);
        });
        
        await context.Login(2018109492, "Qbot@KW#2018");
        await Task.Delay(5000);
        var builder = new MessageBuilder().Text("Awoo!");
        var message = await context.SendFriendMessage(1136772134, builder.Build());
        
        await Task.Delay(-1);
    }
}