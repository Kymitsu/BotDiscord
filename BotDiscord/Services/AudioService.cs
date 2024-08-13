using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using Discord;
using Discord.Audio;
using System.Net.Http;
using Discord.Audio.Streams;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BotDiscord.Services
{
    public class AudioService
    {
        private readonly ConcurrentDictionary<ulong, IAudioClient> ConnectedChannels = new ConcurrentDictionary<ulong, IAudioClient>();
        private CancellationTokenSource cancellationToken = new CancellationTokenSource();
        private PauseTokenSource pauseToken = new PauseTokenSource();
        private bool skipAudio = false;

        public Queue<string> Playlist { get; set; } = new Queue<string>();

        public double Volume { get; set; } = 0.5;
        public bool IsMute { get; set; } = false;
        public bool IsBotPlaying { get; set; } = false;
        public bool IsBotPaused { get { return pauseToken.IsPaused; } }
        public string CurrentAudio { get; private set; }
        public bool IsLooping { get; set; } = false;

        public async Task JoinAudio(IGuild guild, IVoiceChannel target)
        {
            IAudioClient client;
            if (ConnectedChannels.TryGetValue(guild.Id, out client))
            {
                return;
            }
            if (target.Guild.Id != guild.Id)
            {
                return;
            }

            var audioClient = await target.ConnectAsync();

            if (ConnectedChannels.TryAdd(guild.Id, audioClient))
            {
                //await Log(LogSeverity.Info, $"Connected to voice on {guild.Name}.");
            }
        }

        public async Task LeaveAudio(IGuild guild)
        {
            IAudioClient client;
            if (ConnectedChannels.TryRemove(guild.Id, out client))
            {
                await client.StopAsync();
                //await Log(LogSeverity.Info, $"Disconnected from voice on {guild.Name}.");
            }
        }

        public async Task AddToPlaylist(string path)
        {
            Playlist.Enqueue(path);
        }

        public async Task SendAudioAsync(IGuild guild, IMessageChannel channel, string file)
        {
            Playlist.Enqueue(file);

            IAudioClient client;
            if (ConnectedChannels.TryGetValue(guild.Id, out client))
            {
                IsBotPlaying = true;
                while (Playlist.Count > 0 && !cancellationToken.IsCancellationRequested)
                {
                    CurrentAudio = IsLooping ? Playlist.Peek() : Playlist.Dequeue();
                    string path = $"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}Sounds{Path.DirectorySeparatorChar}{CurrentAudio}";
                    if (!File.Exists(path))
                    {
                        IsBotPlaying = false;
                        await channel.SendMessageAsync("File does not exist.");
                        return;
                    }

                    //await Log(LogSeverity.Debug, $"Starting playback of {path} in {guild.Name}");
                    using (var ffmpeg = CreateProcess(path))
                    using (AudioOutStream stream = client.CreatePCMStream(AudioApplication.Music))
                    {
                        try
                        {
                            //await ffmpeg.StandardOutput.BaseStream.CopyToAsync(stream, 81920, cancellationToken.Token);

                            int blockSize = 3840; // The size of bytes to read per frame; 1920 for mono
                            byte[] buffer = new byte[blockSize];
                            byte[] gainBuffer = new byte[blockSize];
                            int byteCount;

                            while (!cancellationToken.IsCancellationRequested)
                            {
                                if (pauseToken.IsPaused) await pauseToken.WaitWhilePausedAsync();
                                if (skipAudio)
                                {
                                    skipAudio = false;
                                    break;
                                }

                                byteCount = ffmpeg.StandardOutput.BaseStream.Read(buffer, 0, blockSize);

                                if (byteCount == 0) // FFmpeg did not output anything
                                    break;

                                for (int i = 0; i < blockSize / 2; ++i)
                                {

                                    // convert to 16-bit
                                    short sample = (short)(buffer[i * 2 + 1] << 8 | buffer[i * 2]);

                                    // scale
                                    if (!IsMute)
                                    {
                                        sample = (short)(sample * Volume + 0.5);
                                    }
                                    else
                                    {
                                        sample = (short)(sample * 0 + 0.5);
                                    }

                                    // back to byte[]
                                    buffer[i * 2 + 1] = (byte)(sample >> 8);
                                    buffer[i * 2] = (byte)(sample & 0xff);
                                }

                                await stream.WriteAsync(buffer, 0, byteCount);
                            }

                        }
                        finally
                        {
                            await stream.FlushAsync();
                        }
                    }
                }
                IsBotPlaying = false;
                cancellationToken = new CancellationTokenSource();
            }
        }

        public async Task StopAudioAsync()
        {
            Playlist.Clear();
            cancellationToken.Cancel();
        }

        public async Task PauseAudioAsync()
        {
            pauseToken.IsPaused = !pauseToken.IsPaused;
        }

        public async Task SkipAudioAsync()
        {
            if (IsLooping) Playlist.Dequeue();
            skipAudio = true;
        }

        private Process CreateProcess(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
        }
    }
}