using CommunityToolkit.Mvvm.Messaging.Messages;

namespace WIMS.Core.Events;

public class ViewModelChangedMessage : ValueChangedMessage<Type>
{
    public ViewModelChangedMessage(object sender, Type viewModelType) : base(viewModelType) => Sender = sender;
    public object Sender { get; }
}

public class DataChangedMessage : ValueChangedMessage<string>
{
    public DataChangedMessage(object sender, string entityName) : base(entityName) => Sender = sender;
    public object Sender { get; }
}
