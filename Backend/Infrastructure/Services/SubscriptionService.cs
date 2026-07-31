using Application.DTOs.Subscription;
using Application.Interfaces.Services;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;

namespace Infrastructure.Services
{
    public class SubscriptionService : ISubscriptionService
    {

        private readonly SonaraDbContext _db; 
        private readonly IMapper _mapper;

        public SubscriptionService(SonaraDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<SubscriptionDto>> GetAllSubscriptionsAsync(CancellationToken ct = default)
        {
            return await _db.Subscriptions
                .OrderByDescending(s => s.Price)
                .ProjectTo<SubscriptionDto>(_mapper.ConfigurationProvider)
                .ToListAsync(ct);
        }

        public async Task<SubscriptionDto?> GetSubscriptionsByIdAsync(Guid subscriptionId, CancellationToken ct = default)
        {
            return await _db.Subscriptions
                .Where(s => s.Id == subscriptionId)
                .ProjectTo<SubscriptionDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<SubscriptionDto?> GetUserSubscriptionAsync(Guid userId, CancellationToken ct = default)
        {
            return await _db.Users
                   .AsNoTracking()
                   .Where(u => u.Id == userId)
                   .Select(u => u.Subscription) 
                   .ProjectTo<SubscriptionDto>(_mapper.ConfigurationProvider)
                   .FirstOrDefaultAsync(ct);
        }

        public async Task<bool> RemoveSubscriptionAsync(Guid subscriptionId, CancellationToken ct = default)
        {
            var subscription = await _db.Subscriptions
                .FirstOrDefaultAsync(s => s.Id == subscriptionId, ct);

            if (subscription == null)
                return false;

            var freeSubscriptionId = await _db.Subscriptions
                .Where(s => s.Name == "Free") 
                .Select(s => s.Id)
                .FirstOrDefaultAsync(ct);

            if (freeSubscriptionId == Guid.Empty || freeSubscriptionId == subscriptionId)
            {
                throw new InvalidOperationException("Неможливо видалити таку підписку або не знайдено дефолтну підписку Free.");
            }

            var usersWithSubscription = await _db.Users
                .Where(u => u.SubscriptionId == subscriptionId)
                .ToListAsync(ct);

            foreach (var user in usersWithSubscription)
            {
                user.SubscriptionId = freeSubscriptionId;
            }

            _db.Subscriptions.Remove(subscription);

            await _db.SaveChangesAsync(ct);

            return true;
        }

        public async Task<SubscriptionDto?> UpdateSubscriptionAsync(Guid subscriptionId, UpdateSubscriptionDto dto, CancellationToken ct = default)
        {
            var subscription = await _db.Subscriptions
                .FirstOrDefaultAsync(s => s.Id == subscriptionId, ct);

            if (subscription == null)
                return null;

            _mapper.Map(dto, subscription);

            await _db.SaveChangesAsync(ct);

            return _mapper.Map<SubscriptionDto>(subscription);
        }

        public async Task<SubscriptionDto> CreateSubscriptionAsync(CreateSubscriptionDto dto, CancellationToken ct = default)
        {
            var subscription = _mapper.Map<Subscription>(dto);

            await _db.Subscriptions.AddAsync(subscription, ct);

            await _db.SaveChangesAsync(ct);

            return _mapper.Map<SubscriptionDto>(subscription);
        }
    }
}
