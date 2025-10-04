using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using WinUI3Template.Core.Models;

namespace WinUI3Template.Core.Contracts.Services;

public interface IBackdropSelectorService
{
    BackdropType BackdropType { get; }

    public event EventHandler<BackdropType>? BackdropTypeChanged;

    Task SetRequestedBackdropTypeAsync(Window window);

    Task SetBackdropTypeAsync(BackdropType type);
}
