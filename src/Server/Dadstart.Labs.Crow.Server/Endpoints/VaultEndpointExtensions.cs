namespace Dadstart.Labs.Crow.Server.Endpoints;

using Microsoft.AspNetCore.Http.HttpResults;

public static class VaultEndpointExtensions
{
    public static void MapVaultEndpoints(this IEndpointRouteBuilder app)
    {
        _ = app.MapGet(VaultApiRoutes.SetupState, async Task<Ok<VaultSetupState>> (IVaultSecurityService security, CancellationToken ct)
            => TypedResults.Ok(await security.GetSetupStateAsync(ct)));

        _ = app.MapPost(VaultApiRoutes.Setup, async Task<Results<Ok<VaultSetupState>, BadRequest<string>>> (VaultSetupRequest request, IVaultSecurityService security, CancellationToken ct) =>
        {
            try
            {
                var state = await security.ConfigureAsync(request, ct);
                return TypedResults.Ok(state);
            }
            catch (InvalidOperationException ex)
            {
                return TypedResults.BadRequest(ex.Message);
            }
        });

        _ = app.MapPost(VaultApiRoutes.Unlock, async Task<Results<Ok<UnlockResponse>, UnauthorizedHttpResult>> (UnlockRequest request, IVaultSecurityService security, CancellationToken ct) =>
        {
            try
            {
                var response = await security.UnlockAsync(request, ct);
                return TypedResults.Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return TypedResults.Unauthorized();
            }
        });

        _ = app.MapPost(VaultApiRoutes.Biometric, async Task<Results<Ok, UnauthorizedHttpResult>> (HttpContext context, BiometricRegistrationRequest payload, IVaultSecurityService security, CancellationToken ct) =>
        {
            if (!context.Request.Headers.TryGetValue(VaultApiRoutes.SessionHeader, out var value))
            {
                return TypedResults.Unauthorized();
            }

            await security.RegisterBiometricAsync(value.ToString(), payload.DeviceId, payload.BiometricToken, ct);
            return TypedResults.Ok();
        }).AddEndpointFilter<SessionValidationFilter>();

        MapNoteEndpoints(app);
        MapPasswordEndpoints(app);
        MapReminderEndpoints(app);
    }

    static void MapNoteEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(VaultApiRoutes.Notes).AddEndpointFilter<SessionValidationFilter>();

        _ = group.MapGet("/", async Task<Ok<IReadOnlyList<SecureNote>>> (string? query, IVaultRepository repository, CancellationToken ct)
            => TypedResults.Ok(await repository.GetNotesAsync(query, ct)));

        _ = group.MapPost("/", async Task<Ok<SecureNote>> (NoteMutation mutation, IVaultRepository repository, CancellationToken ct)
            => TypedResults.Ok(await repository.UpsertNoteAsync(mutation, ct)));

        _ = group.MapDelete("/{id:guid}", async Task<Results<Ok, NotFound>> (Guid id, IVaultRepository repository, CancellationToken ct) =>
        {
            var note = await repository.GetNoteAsync(id, ct);
            if (note is null)
            {
                return TypedResults.NotFound();
            }

            await repository.DeleteNoteAsync(id, ct);
            return TypedResults.Ok();
        });
    }

    static void MapPasswordEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(VaultApiRoutes.Passwords).AddEndpointFilter<SessionValidationFilter>();

        _ = group.MapGet("/", async Task<Ok<IReadOnlyList<PasswordEntry>>> (string? query, IVaultRepository repository, CancellationToken ct)
            => TypedResults.Ok(await repository.GetPasswordsAsync(query, ct)));

        _ = group.MapPost("/", async Task<Ok<PasswordEntry>> (PasswordMutation mutation, IVaultRepository repository, CancellationToken ct)
            => TypedResults.Ok(await repository.UpsertPasswordAsync(mutation, ct)));

        _ = group.MapDelete("/{id:guid}", async Task<Results<Ok, NotFound>> (Guid id, IVaultRepository repository, CancellationToken ct) =>
        {
            var existing = await repository.GetPasswordAsync(id, ct);
            if (existing is null)
            {
                return TypedResults.NotFound();
            }

            await repository.DeletePasswordAsync(id, ct);
            return TypedResults.Ok();
        });
    }

    static void MapReminderEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(VaultApiRoutes.Reminders).AddEndpointFilter<SessionValidationFilter>();

        _ = group.MapGet("/", async Task<Ok<IReadOnlyList<ReminderEntry>>> (IVaultRepository repository, CancellationToken ct)
            => TypedResults.Ok(await repository.GetRemindersAsync(ct)));

        _ = group.MapPost("/", async Task<Ok<ReminderEntry>> (ReminderMutation mutation, IVaultRepository repository, CancellationToken ct)
            => TypedResults.Ok(await repository.UpsertReminderAsync(mutation, ct)));

        _ = group.MapDelete("/{id:guid}", async Task<Results<Ok, NotFound>> (Guid id, IVaultRepository repository, CancellationToken ct) =>
        {
            var existing = await repository.GetReminderAsync(id, ct);
            if (existing is null)
            {
                return TypedResults.NotFound();
            }

            await repository.DeleteReminderAsync(id, ct);
            return TypedResults.Ok();
        });
    }
}

