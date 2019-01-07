﻿using DockerYoutubeDL.DAL;
using DockerYoutubeDL.Models;
using DockerYoutubeDL.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DockerYoutubeDL.Services
{
    public class NotificationService
    {
        // Factory is used since the service is instantiated once as singleton and therefor 
        // outlives the "normal" session lifetime of the dbcontext.
        private IDesignTimeDbContextFactory<DownloadContext> _factory;
        private ILogger _logger;
        private IHubContext<UpdateHub> _hub;
        private DownloadPathGenerator _pathGenerator;

        private Policy _notificationPolicy;

        public NotificationService(
            IDesignTimeDbContextFactory<DownloadContext> factory,
            ILogger<NotificationService> logger,
            IHubContext<UpdateHub> hub,
            DownloadPathGenerator pathGenerator)
        {
            if (factory == null || logger == null || hub == null ||
                pathGenerator == null)
            {
                throw new ArgumentException();
            }

            _factory = factory;
            _logger = logger;
            _hub = hub;
            _pathGenerator = pathGenerator;

            // The same policy is used for all notification attempts.
            _notificationPolicy = Policy.Handle<Exception>()
                .WaitAndRetryAsync(5, (count) => TimeSpan.FromSeconds(count + 1), (e, retryCount, context) =>
                {
                    _logger.LogError(e, "Retry error: " + context["errorMessage"] as string);
                });
        }

        public async Task NotifyClientAboutReceivedDownloadInformationAsync(YoutubeDlOutputInfo outputInfo)
        {
            // Notify the matching client about the received download information.
            await _notificationPolicy.ExecuteAsync(
                async (context) =>
                {
                    _logger.LogDebug($"Notifying clients about the received download info for result with id={outputInfo.DownloadResultIdentifier}.");

                    await _hub.Clients.All.SendAsync(nameof(IUpdateClient.ReceivedDownloadInfo), outputInfo);
                },
                new Dictionary<string, object>()
                {
                    { "errorMessage", $"Error while notifying clients about the received download info for result with id={outputInfo.DownloadResultIdentifier}." }
                }
            );
        }

        public async Task NotifyClientAboutFailedDownloadAsync(YoutubeDlOutputInfo outputInfo)
        {
            // Notify the matching client about the failed download.
            await _notificationPolicy.ExecuteAsync(
                async (context) =>
                {
                    _logger.LogDebug($"Notifying clients about the failed download task with id={outputInfo.DownloadTaskIdentifier}.");

                    await _hub.Clients.All.SendAsync(nameof(IUpdateClient.DownloadFailed), outputInfo);
                },
                new Dictionary<string, object>()
                {
                    { "errorMessage", $"Error while notifying clients about the failed download task with id={outputInfo.DownloadTaskIdentifier}." }
                }
            );
        }

        public async Task NotifyClientAboutStartedDownloadAsync(Guid downloadTaskId, Guid downloadResultId)
        {
            // Notify the matching client about the started download.
            await _notificationPolicy.ExecuteAsync(
                async (context) =>
                {
                    _logger.LogDebug($"Notifying clients about the started download task with id={downloadTaskId}, result.id={downloadResultId}.");

                    await _hub.Clients.All.SendAsync(nameof(IUpdateClient.DownloadStarted), downloadTaskId, downloadResultId);
                },
                new Dictionary<string, object>()
                {
                    { "errorMessage", $"Error while notifying clients about the started download task with id={downloadTaskId}, result.id={downloadResultId}." }
                }
            );
        }

        public async Task NotifyClientAboutDownloadProgressAsync(Guid downloadTaskId, Guid downloadResultId, double percentage)
        {
            // Notify the matching client about the download progress.
            await _notificationPolicy.ExecuteAsync(
                async (context) =>
                {
                    _logger.LogDebug($"Notifying clients about progress {percentage} of task={downloadTaskId}, result={downloadResultId}.");

                    await _hub.Clients.All.SendAsync(nameof(IUpdateClient.DownloadProgress), downloadTaskId, downloadResultId, percentage);
                },
                new Dictionary<string, object>()
                {
                    { "errorMessage", $"Error while notifying clients about progress {percentage} of task={downloadTaskId}, result={downloadResultId}." }
                }
            );
        }

        public async Task NotifyClientAboutDownloadConversionAsync(Guid downloadTaskId, Guid downloadResultId)
        {
            // Notify the matching client about the conversion of the file.
            await _notificationPolicy.ExecuteAsync(
                async (context) =>
                {
                    _logger.LogDebug($"Notifying clients about conversion with id={downloadResultId}.");

                    await _hub.Clients.All.SendAsync(nameof(IUpdateClient.DownloadConversion), downloadTaskId, downloadResultId);
                },
                new Dictionary<string, object>()
                {
                    { "errorMessage", $"Error while notifying clients about conversion with id={downloadResultId}." }
                }
            );
        }

        public async Task NotifyClientAboutFinishedDownloadAsync(Guid downloadTaskId, Guid downloadResultId)
        {
            // Notify the matching client about the finished download.
            await _notificationPolicy.ExecuteAsync(
                async (context) =>
                {
                    _logger.LogDebug($"Notifying clients about result with id={downloadResultId}.");

                    await _hub.Clients.All.SendAsync(nameof(IUpdateClient.DownloadFinished), downloadTaskId, downloadResultId);
                },
                new Dictionary<string, object>()
                {
                    { "errorMessage", $"Error while notifying clients about result with id={downloadResultId}." }
                }
            );
        }

        public async Task NotifyClientAboutInterruptedDownloadAsync(Guid downloadTaskId)
        {
            // Notify the matching client about the finished download.
            await _notificationPolicy.ExecuteAsync(
                async (context) =>
                {
                    _logger.LogDebug($"Notifying client about interrupt of task with id={downloadTaskId}.");

                    await _hub.Clients.All.SendAsync(nameof(IUpdateClient.DownloadInterrupted), downloadTaskId);
                },
                new Dictionary<string, object>()
                {
                    { "errorMessage", $"Error while notifying clients about interrupt of task with id={downloadTaskId}." }
                }
            );
        }

        public async Task NotifyClientAboutDownloaderError(Guid downloadTaskId)
        {
            // Notify the matching client about the failure.
            await _notificationPolicy.ExecuteAsync(
                async (context) =>
                {
                    _logger.LogDebug($"Notifying clients about the downloader error of task {downloadTaskId}.");

                    await _hub.Clients.All.SendAsync(nameof(IUpdateClient.DownloaderError), downloadTaskId);
                },
                new Dictionary<string, object>()
                {
                    { "errorMessage", $"Error while notifying clients about the downloader error of task {downloadTaskId}." }
                }
            );
        }
    }
}
