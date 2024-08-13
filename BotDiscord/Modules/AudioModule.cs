using BotDiscord.Services;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BotDiscord.Modules
{
    [Summary("Audio")]
    public class AudioModule : ModuleBase<ICommandContext>
    {
        private readonly AudioService _service;
        private CommandService _commandService;

        public AudioModule(CommandService commandService, AudioService service)
        {
            _commandService = commandService;
            _service = service;
        }

        // You *MUST* mark these commands with 'RunMode.Async'
        // otherwise the bot will not respond until the Task times out.
        [Command("♪join", RunMode = RunMode.Async)]
        [Alias("$join")]
        [Summary("Fait rejoindre le bot dans un canal vocal")]
        public async Task JoinCmd()
        {
            await Context.Message.DeleteAsync();
            await _service.JoinAudio(Context.Guild, (Context.User as IVoiceState).VoiceChannel);
        }

        [Command("♪leave", RunMode = RunMode.Async)]
        [Alias("$leave")]
        public async Task LeaveCmd()
        {
            await Context.Message.DeleteAsync();
            await _service.LeaveAudio(Context.Guild);
        }

        [Command("♪play", RunMode = RunMode.Async)]
        [Alias("$play")]
        public async Task PlayCmd(params string[] s)
        {
            string file = s[0];
            if (s.Length == 2 && s[1] == "loop")
                _service.IsLooping = true;

            await Context.Message.DeleteAsync();
            if (!_service.IsBotPlaying)
            {
                await Context.Channel.SendMessageAsync($"Currently playing: {file}.");
                await _service.SendAudioAsync(Context.Guild, Context.Channel, file);
            }
            else
            {
                await _service.StopAudioAsync();
                await Task.Delay(500);
                await _service.SendAudioAsync(Context.Guild, Context.Channel, file);
                //await _service.AddToPlaylist(file);
                //await Context.Channel.SendMessageAsync($"{file} added to playlist. {_service.Playlist.Count} audio in playlist.");
            }

        }

        [Command("♪loop", RunMode = RunMode.Async)]
        [Alias("$loop")]
        public async Task LoopCmd()
        {
            _service.IsLooping = !_service.IsLooping;
            if (_service.IsLooping)
                await Context.Channel.SendMessageAsync("Looping current audio");
            else
                await Context.Channel.SendMessageAsync("Stopped looping current audio");
        }

        [Command("♪add", RunMode = RunMode.Async)]
        [Alias("$add")]
        public async Task AddCmd([Remainder] string file)
        {
            await Context.Message.DeleteAsync();
            await _service.AddToPlaylist(file);
            await Context.Channel.SendMessageAsync($"{file} added to playlist. {_service.Playlist.Count} audio in playlist.");
        }

        [Command("♪next", RunMode = RunMode.Async)]
        [Alias("$next")]
        public async Task NextCmd()
        {
            await Context.Message.DeleteAsync();
            if (_service.IsBotPlaying)
            {
                await Context.Channel.SendMessageAsync($"Next Audio : {_service.Playlist.Peek()}");
            }
            else
            {
                await Context.Channel.SendMessageAsync("AudioBot is not playing!");
            }
        }

        [Command("♪skip", RunMode = RunMode.Async)]
        [Alias("$skip")]
        public async Task SkipCmd()
        {
            await Context.Message.DeleteAsync();
            if (_service.IsBotPlaying)
            {
                _ = _service.SkipAudioAsync();
                await Context.Channel.SendMessageAsync($"{_service.CurrentAudio} Skipped");
            }
            else
            {
                await Context.Channel.SendMessageAsync("AudioBot is not playing!");
            }
        }

        [Command("♪volume", RunMode = RunMode.Async)]
        [Alias("♪vol", "$volume", "$vol")]
        public async Task VolumeCmd(string volstr)
        {
            await Context.Message.DeleteAsync();
            double volume;
            //Try parsing in the current culture
            if (!double.TryParse(volstr, NumberStyles.Any, CultureInfo.CurrentCulture, out volume) &&
                //Then in neutral language
                !double.TryParse(volstr, NumberStyles.Any, CultureInfo.InvariantCulture, out volume))
            {
                volume = 0.5;
            }

            if (volume > 1.5)
                volume = 1.5;

            if (volume <= 0)
            {
                _service.IsMute = true;
                await Context.Channel.SendMessageAsync("AudioBot muted!");
            }
            else
            {
                _service.Volume = volume;
                await Context.Channel.SendMessageAsync($"Volume set to {volume}");
            }
        }

        [Command("♪mute", RunMode = RunMode.Async)]
        [Alias("$mute")]
        public async Task MuteCmd()
        {
            await Context.Message.DeleteAsync();
            if (_service.IsBotPlaying)
            {
                if (!_service.IsMute)
                {
                    await Context.Channel.SendMessageAsync("AudioBot muted!");
                }
                else
                {
                    await Context.Channel.SendMessageAsync("AudioBot unmuted!");
                }
                _service.IsMute = !_service.IsMute;
            }
            else
            {
                await Context.Channel.SendMessageAsync("AudioBot is not playing!");
            }
        }


        [Command("♪stop", RunMode = RunMode.Async)]
        [Alias("$stop")]
        public async Task StopCmd()
        {
            await Context.Message.DeleteAsync();
            if (_service.IsBotPlaying)
            {
                await _service.StopAudioAsync();
                await Context.Channel.SendMessageAsync("AudioBot stopped!");
            }
            else
            {
                await Context.Channel.SendMessageAsync("AudioBot is not playing!");
            }
        }

        [Command("♪pause", RunMode = RunMode.Async)]
        [Alias("$pause")]
        public async Task PauseCmd()
        {
            await Context.Message.DeleteAsync();
            if (_service.IsBotPlaying)
            {
                if (!_service.IsBotPaused)
                    await Context.Channel.SendMessageAsync("AudioBot paused!");
                else
                    await Context.Channel.SendMessageAsync("AudioBot resumed!");
                await _service.PauseAudioAsync();
            }
            else
            {
                await Context.Channel.SendMessageAsync("AudioBot is not playing!");
            }
        }

        [Command("♪upload", RunMode = RunMode.Async)]
        [Alias("$upload")]
        public async Task UploadCmd()
        {
            if (Context.Message.Attachments.Any())
            {
                IAttachment attachment = Context.Message.Attachments.First();
                using (HttpClient hclient = new HttpClient())
                {
                    Stream stream;
                    try
                    {
                        stream = await hclient.GetStreamAsync(attachment.Url);
                    }
                    catch (Exception)
                    {
                        try
                        {
                            stream = await hclient.GetStreamAsync(attachment.ProxyUrl);
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }

                    FileStream fs = File.Create($"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}Sounds{Path.DirectorySeparatorChar}{attachment.Filename}");
                    stream.CopyTo(fs);

                    await Context.Message.DeleteAsync();
                    await Context.Channel.SendMessageAsync("File uploaded successfully");
                }
            }
        }

        [Command("♪help"), Summary("Liste de toutes les commandes audio")]
        [Alias("$help")]
        public async Task Help()
        {
            await Context.Message.DeleteAsync();
            string helpLine = "```";
            foreach (var module in _commandService.Modules)
            {
                if (module.Name == "AudioModule")
                {
                    string moduleName = module.Name;
                    moduleName += moduleName != string.Empty ? " " : string.Empty;
                    foreach (var command in module.Commands)
                    {
                        helpLine += string.Format("{2}{0} \t {1} : Ex => {2}{0} ", command.Name, command.Summary, moduleName);
                        foreach (var parameter in command.Parameters)
                        {
                            helpLine += "[" + parameter.Summary + "] ";
                        }
                        helpLine += Environment.NewLine;
                    }
                }
            }
            helpLine += "```";
            await Context.Channel.SendMessageAsync(helpLine);
        }

    }
}
