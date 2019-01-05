﻿using DockerYoutubeDL.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DockerYoutubeDL.SignalR
{
    public interface IUpdateClient
    {
        Task ReceivedDownloadInfo(DownloadResult downloadResult);

        // Id of the task (received when it got queued up) and id of the result corresponding to the task.
        Task DownloadStarted(Guid taskIdentifier, Guid taskResultIdentifier);

        Task DownloadProgress(Guid taskIdentifier, Guid taskResultIdentifier, double percentage);

        Task DownloadConversion(Guid taskIdentifier, Guid taskResultIdentifier);

        // Id of the task (received when it got queued up) and id of the result corresponding to the task.
        Task DownloadFinished(Guid taskIdentifier, Guid taskResultIdentifier);

        Task DownloadFailed(Guid taskIdentifier);

        // Test if a client disconnected or just downloaded a file.
        Task Ping();
    }
}
