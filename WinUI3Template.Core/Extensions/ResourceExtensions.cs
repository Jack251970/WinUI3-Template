// Copyright (c) 2024 Jack251970
// Licensed under the GPL License. See the LICENSE.

using System.Collections.Concurrent;
using Microsoft.Windows.ApplicationModel.Resources;

namespace WinUI3Template.Core.Extensions;

/// <summary>
/// Provides static extension for resources management, support string caching.
/// </summary>
public static class ResourceExtensions
{
    private static readonly ConcurrentDictionary<string, string> cachedResources = new();

    private static readonly ResourceMap HostResourceMap = new ResourceManager().MainResourceMap;

    private static readonly Dictionary<string, ResourceMap> resourcesTrees = new()
    {
        { Constants.DefaultResourceFileName, HostResourceMap.TryGetSubtree(Constants.DefaultResourceFileName) }
    };

    #region resource management

    /// <summary>
    /// Add resource file of the host project.
    /// </summary>
    /// <param name="resourceFileName">
    /// The name of the resource file.
    /// </param>
    public static void AddLocalResource(string resourceFileName)
    {
        var resourceMap = HostResourceMap.TryGetSubtree(resourceFileName);
        resourcesTrees.Add(resourceFileName, resourceMap);
    }

    /// <summary>
    /// Get resource map by resource file name.
    /// </summary>
    /// <param name="resourceFileName">
    /// The name of the resource file or the assembly of the extension project.
    /// </param>
    /// <returns></returns>
    public static ResourceMap? TryGetResourceMap(string resourceFileName = Constants.DefaultResourceFileName)
    {
        return resourcesTrees.TryGetValue(resourceFileName, out var resourceMap) ? resourceMap : null;
    }

    #endregion

    #region extension methods

    public static string GetLocalizedString(this string resourceKey, string resourceFileName = Constants.DefaultResourceFileName)
    {
        // Fix resource key
        resourceKey = resourceKey.Replace(".", "/");

        // Try to get cached value
        var cachedResourceKey = $"{resourceFileName}/{resourceKey}";
        if (cachedResources.TryGetValue(cachedResourceKey, out var value))
        {
            return value;
        }

        // Get resource value
        var resourcesTree = resourcesTrees[resourceFileName];
        value = resourcesTree.TryGetValue(resourceKey)?.ValueAsString;

        // Return empty string if the resource key is not found.
        return cachedResources[cachedResourceKey] = value ?? string.Empty;
    }

    #endregion
}
