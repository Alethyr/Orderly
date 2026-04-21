using Core.Entities;

namespace Core.Intrefaces;

public interface IPaymentService
{
   Task<ShoppingCart?> CreateOrUpdatePaymentIntent(string cardId);
}
