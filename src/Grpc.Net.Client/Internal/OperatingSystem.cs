#region Copyright notice and license

// Copyright 2019 The gRPC Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System.Runtime.InteropServices;

namespace Grpc.Net.Client.Internal;

internal interface IOperatingSystem
{
    bool IsBrowser { get; } = false;
    bool IsAndroid { get; } = false;
    bool IsWindows { get; } = true;
    bool IsWindowsServer { get; }
    Version OSVersion { get; }
}

internal sealed class OperatingSystem : IOperatingSystem
{
    public static readonly OperatingSystem Instance = new OperatingSystem();

    private readonly Lazy<bool> _isWindowsServer;

    public bool IsBrowser { get; } = false;
    public bool IsAndroid { get; } = false;
    public bool IsWindows { get; } = true;
    public bool IsWindowsServer => _isWindowsServer.Value;
    public Version OSVersion { get; }

    private OperatingSystem()
    {
#if NET5_0_OR_GREATER
        OSVersion = Environment.OSVersion.Version;

        _isWindowsServer = new Lazy<bool>(() =>
        {
            // RtlGetVersion is not available on UWP. Check it first.
            if (IsWindows && !Native.IsUwp(RuntimeInformation.FrameworkDescription, Environment.OSVersion.Version))
            {
                Native.DetectWindowsVersion(out _, out var isWindowsServer);
                return isWindowsServer;
            }

            return false;
        }, LazyThreadSafetyMode.ExecutionAndPublication);
#else

        if (IsWindows && !Native.IsUwp(RuntimeInformation.FrameworkDescription, Environment.OSVersion.Version))
        {
            Native.DetectWindowsVersion(out var windowsVersion, out var windowsServer);
            OSVersion = windowsVersion;
            _isWindowsServer = new Lazy<bool>(() => windowsServer, LazyThreadSafetyMode.ExecutionAndPublication);
        }
        else
        {
            OSVersion = Environment.OSVersion.Version;
            _isWindowsServer = new Lazy<bool>(() => false, LazyThreadSafetyMode.ExecutionAndPublication);
        }
#endif
    }
}
