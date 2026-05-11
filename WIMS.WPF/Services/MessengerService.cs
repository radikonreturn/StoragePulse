using CommunityToolkit.Mvvm.Messaging;
using WIMS.Core.Interfaces;

namespace WIMS.WPF.Services;

public class MessengerService : IMessengerService
{
    private readonly IMessenger _messenger = WeakReferenceMessenger.Default;

    public void Send<TMessage>(TMessage message) where TMessage : class
        => _messenger.Send(message);

    public void Register<TMessage>(object recipient, Action<TMessage> action) where TMessage : class
        => _messenger.Register<TMessage>(recipient, (r, m) => action(m));

    public void Unregister<TMessage>(object recipient) where TMessage : class
        => _messenger.Unregister<TMessage>(recipient);
}
