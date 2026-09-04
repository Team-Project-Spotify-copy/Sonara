using Application.DTOs.Subscription;
using Application.Interfaces.Services;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly SonaraDbContext _db;
    private readonly IMapper _mapper;

    public SubscriptionService(SonaraDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<SubscriptionPlanDto>> GetAllPlansAsync(CancellationToken ct = default)
    {
        return await _db.SubscriptionPlans
            .AsNoTracking()
            .OrderBy(p => p.Price)
            .ProjectTo<SubscriptionPlanDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }

    public async Task<SubscriptionPlanDto?> GetPlanByIdAsync(Guid planId, CancellationToken ct = default)
    {
        return await _db.SubscriptionPlans
            .AsNoTracking()
            .Where(p => p.Id == planId)
            .ProjectTo<SubscriptionPlanDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<SubscriptionPlanDto> CreatePlanAsync(CreateSubscriptionPlanDto dto, CancellationToken ct = default)
    {
        var plan = _mapper.Map<SubscriptionPlan>(dto);

        await _db.SubscriptionPlans.AddAsync(plan, ct);
        await _db.SaveChangesAsync(ct);

        return _mapper.Map<SubscriptionPlanDto>(plan);
    }

    public async Task<SubscriptionPlanDto?> UpdatePlanAsync(Guid planId, UpdateSubscriptionPlanDto dto, CancellationToken ct = default)
    {
        var plan = await _db.SubscriptionPlans.FirstOrDefaultAsync(p => p.Id == planId, ct);

        if (plan == null)
            return null;

        _mapper.Map(dto, plan);
        await _db.SaveChangesAsync(ct);

        return _mapper.Map<SubscriptionPlanDto>(plan);
    }

    public async Task<bool> RemovePlanAsync(Guid planId, CancellationToken ct = default)
    {
        var plan = await _db.SubscriptionPlans.FirstOrDefaultAsync(p => p.Id == planId, ct);

        if (plan == null)
            return false;

        _db.SubscriptionPlans.Remove(plan);
        await _db.SaveChangesAsync(ct);

        return true;
    }


    public async Task<UserSubscriptionDto?> GetUserSubscriptionAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user == null || user.ActiveSubscriptionId == null)
            return null;

        return await _db.UserSubscriptions
            .AsNoTracking()
            .Where(us => us.Id == user.ActiveSubscriptionId)
            .ProjectTo<UserSubscriptionDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<UserSubscriptionDto> ProcessBlockchainPurchaseAsync(Guid userId, byte planTypeByte, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new InvalidOperationException("User not found");

        string planName = planTypeByte switch
        {
            0 => "Individual",
            1 => "Duo",
            2 => "Family",
            _ => throw new InvalidOperationException($"Unknown plan type byte: {planTypeByte}")
        };

        var plan = await _db.SubscriptionPlans.FirstOrDefaultAsync(p => p.Name == planName, ct)
            ?? throw new InvalidOperationException($"Plan {planName} not found");

        var newSubscription = new UserSubscription
        {
            Id = Guid.NewGuid(),
            PlanId = plan.Id,
            OwnerId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddMonths(1)
        };

        newSubscription.Members.Add(user);

        await _db.UserSubscriptions.AddAsync(newSubscription, ct);

        user.ActiveSubscriptionId = newSubscription.Id;

        await _db.SaveChangesAsync(ct);

        return _mapper.Map<UserSubscriptionDto>(newSubscription);
    }

    public async Task<bool> InviteToSubscriptionAsync(Guid ownerId, string targetUsername, CancellationToken ct = default)
    {
        var activeSub = await _db.UserSubscriptions
            .Include(s => s.Plan)
            .Include(s => s.Members)
            .FirstOrDefaultAsync(s => s.OwnerId == ownerId, ct);

        if (activeSub == null)
            return false;

        if (activeSub.Members.Count >= activeSub.Plan.MaxSlots)
            return false;

        var targetUser = await _db.Users.FirstOrDefaultAsync(u => u.Username == targetUsername, ct);
        if (targetUser == null)
            return false;

        activeSub.Members.Add(targetUser);
        targetUser.ActiveSubscriptionId = activeSub.Id;

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> LeaveOrRemoveFromSubscriptionAsync(Guid currentUserId, Guid activeSubId, Guid userIdToRemove, CancellationToken ct = default)
    {
        var activeSub = await _db.UserSubscriptions
            .Include(s => s.Members)
            .FirstOrDefaultAsync(s => s.Id == activeSubId, ct);

        if (activeSub == null)
            return false;

        bool isSelfRemoval = currentUserId == userIdToRemove;
        bool isOwner = activeSub.OwnerId == currentUserId;

        if (!isSelfRemoval && !isOwner)
            return false;

        var user = activeSub.Members.FirstOrDefault(u => u.Id == userIdToRemove);
        if (user == null)
            return false;

        if (isOwner && isSelfRemoval) return false;

        activeSub.Members.Remove(user);

        var planTypeFree = await _db.SubscriptionPlans.FirstOrDefaultAsync(p => p.Name == "Free", ct);

        if (planTypeFree == null) return false;

        var personalFreeSub = new UserSubscription
        {
            Id = Guid.NewGuid(),
            OwnerId = user.Id,
            PlanId = planTypeFree.Id,
            ExpiresAt = DateTime.MaxValue,
        };

        _db.UserSubscriptions.Add(personalFreeSub);

        user.ActiveSubscriptionId = personalFreeSub.Id;

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> CancelSubscriptionAsync(Guid currentUserId, Guid activeSubId, CancellationToken ct = default)
    {
        var activeSub = await _db.UserSubscriptions
            .Include(s => s.Members)
            .FirstOrDefaultAsync(s => s.Id == activeSubId, ct);

        if (activeSub == null)
            return false;

        if (activeSub.OwnerId != currentUserId)
            return false;

        var freePlan = await _db.SubscriptionPlans.FirstOrDefaultAsync(p => p.Name == "Free", ct);
        if (freePlan == null)
            return false;

        foreach (var member in activeSub.Members.ToList())
        {
            var personalFreeSub = new UserSubscription
            {
                Id = Guid.NewGuid(),
                OwnerId = member.Id,
                PlanId = freePlan.Id,
                ExpiresAt = DateTime.MaxValue
            };

            _db.UserSubscriptions.Add(personalFreeSub);

            member.ActiveSubscriptionId = personalFreeSub.Id;
        }

        _db.UserSubscriptions.Remove(activeSub);

        await _db.SaveChangesAsync(ct);
        return true;
    }
}