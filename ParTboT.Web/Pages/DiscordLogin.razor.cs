using DSharpPlus;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using PuppeteerSharp;

namespace ParTboT.Web.Pages
{
    public partial class DiscordLogin
    {
        [CascadingParameter]
        public Task<AuthenticationState> AuthState { get; set; }

        protected override async void OnAfterRender(bool firstRender)
        {
            string Token = await ctx.HttpContext.GetTokenAsync("Discord", "access_token");
            DiscordRestClient client = login.Login(ulong.Parse(ctx.HttpContext.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value), Token);

            //Console.WriteLine($"\n\n{(await AuthState).User.Identity!.Name!}'s TOKEN => {Token}\n\n");

            //IReadOnlyList<DiscordGuild> guilds = await client.GetCurrentUserGuildsAsync();
            //Console.WriteLine(string.Join(", ", guilds.Select(g => g.Name)));
        }

        private async Task GetQrCodeAsync()
        {
            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                //Headless = true,
                ExecutablePath = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe"
            });

            var page = await browser.NewPageAsync();
            await page.GoToAsync("https://discord.com/login");
            var qrCode = await page.WaitForSelectorAsync("#app-mount > div.appDevToolsWrapper-1QxdQf > div > div.app-3xd6d0 > div > div > div > div > form > div > div > div.transitionGroup-bPT0qU.qrLogin-1ejtpI");

            //await page.ExposeFunctionAsync("newQR", async (string text) =>
            //{
            //    Console.WriteLine(text);

            //    if (text.ToLower().Contains(args.TriggerWord) && !text.Contains(args.ResponseTemplate))
            //    {
            //        await RespondAsync(args, text);
            //    }

            //    text = text.Replace(args.ResponseTemplate, string.Empty);
            //    await File.AppendAllTextAsync(args.SourceText, text + "\n");
            //});

            await page.EvaluateFunctionAsync
                (
                    $@"() => {{
                                var observer = new MutationObserver((mutations) => {{
                                    for(var mutation of mutations) {{
                                        if(mutation.type 
                                           mutation.addedNodes[0].classList.value === '{{WhatsAppMetadata.MessageLine}}') {{
                                            newChat(mutation.addedNodes[0].querySelector('.copyable-text span').innerText);
                                        }}
                                    }}
                                }});
                                observer.observe(
                                    document.querySelector('{{WhatsAppMetadata.ChatContainer}}'),
                                    );
                            }}"
                );
        }
    }
}
