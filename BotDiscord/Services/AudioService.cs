using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using Discord;
using Discord.Audio;
using System.Net.Http;
//using YoutubeExplode;
//using YoutubeExplode.Models.MediaStreams;
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

        public double Volume { get; set; } = 0.5;

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
                // If you add a method to log happenings from this service,
                // you can uncomment these commented lines to make use of that.
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

        public async Task SendAudioAsync(IGuild guild, IMessageChannel channel, string path)
        {
            path = $"{Directory.GetCurrentDirectory()}\\Sounds\\{path}";
            // Your task: Get a full path to the file if the value of 'path' is only a filename.
            if (!File.Exists(path))
            {
                await channel.SendMessageAsync("File does not exist.");
                return;
            }
            IAudioClient client;
            if (ConnectedChannels.TryGetValue(guild.Id, out client))
            {
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
                            byteCount = ffmpeg.StandardOutput.BaseStream // Access the underlying MemoryStream from the stdout of FFmpeg
                                .Read(buffer, 0, blockSize); // Read stdout into the buffer

                            if (byteCount == 0) // FFmpeg did not output anything
                                break; // Break out of the while(true) loop, since there was nothing to read.

                            for (int i = 0; i < blockSize / 2; ++i)
                            {

                                // convert to 16-bit
                                short sample = (short)((buffer[i * 2 + 1] << 8) | buffer[i * 2]);

                                // scale
                                //const double gain = 0.5; // value between 0 and 1.0
                                sample = (short)(sample * Volume + 0.5);

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
                        cancellationToken = new CancellationTokenSource();
                    }
                }
            }
        }

        public async Task StopAudioAsync()
        {
            cancellationToken.Cancel();
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