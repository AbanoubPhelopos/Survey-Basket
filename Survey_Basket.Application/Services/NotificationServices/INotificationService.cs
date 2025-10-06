namespace Survey_Basket.Application.Services.NotificationServices;

public interface INotificationService
{
    Task SendNewPollsNotification(Guid? pollId = null);
}
