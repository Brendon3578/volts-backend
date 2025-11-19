using Microsoft.EntityFrameworkCore;
using Volts.Domain.Entities;
using Volts.Domain.Enums;
using Volts.Infrastructure.Data;

namespace Volts.Infrastructure.Seed
{
    /// <summary>
    /// DatabaseSeeder preenche todas as tabelas com dados coerentes e aleatórios.
    /// Regras:
    /// - Não duplica dados (verifica se já há registros por tabela e pula o seed correspondente).
    /// - Respeita FKs e relações.
    /// - Gera dados realistas de usuários, organizações, grupos, posições, escalas e voluntários.
    /// - Datas de escalas são geradas nos próximos meses.
    /// </summary>
    public class DatabaseSeeder
    {
        private readonly VoltsDbContext _db;
        private readonly Random _random = new Random();
        private static DateTime EnsureUtc(DateTime dt) => dt.Kind == DateTimeKind.Utc ? dt : DateTime.SpecifyKind(dt, DateTimeKind.Utc);

        // Quantidades base
        private const int UsersCount = 20;
        private const int OrganizationsCount = 5;
        private const int GroupsPerOrganization = 3;
        private const int PositionsPerGroup = 4;
        private const int ShiftsPerGroup = 4;
        private const int VolunteersPerShiftPositionMax = 4;

        public DatabaseSeeder(VoltsDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Executa o seed completo. Idempotente por tabela (não duplica).
        /// </summary>
        public async Task SeedAsync()
        {
            // Em cenários de produção, rode apenas quando necessário.
            // Aqui, para fins de desenvolvimento/demo, chamamos sempre com verificações por tabela.

            await SeedUsersAsync();
            await SeedOrganizationsAsync();
            await SeedOrganizationMembersAsync();
            await SeedGroupsAsync();
            await SeedPositionsAsync();
            await SeedShiftsAsync();
            await SeedShiftPositionsAsync();
            await SeedVolunteersAsync();
        }

        private async Task SeedUsersAsync()
        {
            if (await _db.Users.AnyAsync()) return; // já existe

            var firstNames = new[] { "Bruna", "Carlos", "Daniela", "Eduardo", "Fernanda", "Gabriel", "Helena", "Igor", "Julia", "Leonardo", "Marina", "Nicolas", "Olivia", "Paulo", "Quésia", "Rafael", "Sofia", "Tiago", "Ursula", "Vitor" };
            var lastNames = new[] { "Silva", "Santos", "Oliveira", "Souza", "Lima", "Pereira", "Costa", "Ferraz", "Barbosa", "Rocha", "Almeida" };
            var domains = new[] { "example.com", "mail.com", "test.org" };

            var users = new List<User>();
            for (int i = 0; i < UsersCount; i++)
            {
                var first = firstNames[_random.Next(firstNames.Length)];
                var last = lastNames[_random.Next(lastNames.Length)];
                var email = $"{first.ToLower()}.{last.ToLower()}{_random.Next(100, 999)}@{domains[_random.Next(domains.Length)]}";
                var birth = DateTime.UtcNow.AddYears(-_random.Next(18, 50)).AddDays(_random.Next(0, 365));

                users.Add(new User
                {
                    Name = $"{first} {last}",
                    Email = email,
                    Phone = $"+55 11 9{_random.Next(1000, 9999)}-{_random.Next(1000, 9999)}",
                    Bio = "Usuário de teste gerado pelo seeder.",
                    Password = "hashed-password",
                    Gender = _random.Next(0, 2) == 0 ? "M" : "F",
                    Birthdate = birth
                });
            }

            await _db.Users.AddRangeAsync(users);
            await _db.SaveChangesAsync();
        }

        private async Task SeedOrganizationsAsync()
        {
            if (await _db.Organizations.AnyAsync()) return;

            var creators = await _db.Users.Take(OrganizationsCount).ToListAsync();
            var orgs = new List<Organization>();

            for (int i = 0; i < OrganizationsCount; i++)
            {
                var creator = creators[i % creators.Count];
                orgs.Add(new Organization
                {
                    Name = $"Org {i + 1} - {_random.Next(100, 999)}",
                    Description = "Organização de teste para demonstração de seed.",
                    Email = $"org{i + 1}@example.com",
                    Phone = $"+55 11 {_random.Next(2000, 9999)}-{_random.Next(1000, 9999)}",
                    Address = $"Rua {_random.Next(1, 200)} - São Paulo/SP",
                    CreatedById = creator.Id
                });
            }

            await _db.Organizations.AddRangeAsync(orgs);
            await _db.SaveChangesAsync();
        }

        private async Task SeedOrganizationMembersAsync()
        {
            if (await _db.OrganizationMembers.AnyAsync()) return;

            var users = await _db.Users.ToListAsync();
            var orgs = await _db.Organizations.Include(o => o.CreatedBy).ToListAsync();
            var memberships = new List<OrganizationMember>();

            foreach (var org in orgs)
            {
                // Admin: o criador
                memberships.Add(new OrganizationMember
                {
                    UserId = org.CreatedById,
                    OrganizationId = org.Id,
                    Role = OrganizationRoleEnum.ADMIN,
                    JoinedAt = DateTime.UtcNow
                });

                // Alguns líderes e membros aleatórios
                var shuffled = users.Where(u => u.Id != org.CreatedById).OrderBy(_ => _random.Next()).Take(6).ToList();
                foreach (var (user, idx) in shuffled.Select((u, idx) => (u, idx)))
                {
                    var role = idx < 2 ? OrganizationRoleEnum.LEADER : OrganizationRoleEnum.MEMBER;
                    memberships.Add(new OrganizationMember
                    {
                        UserId = user.Id,
                        OrganizationId = org.Id,
                        Role = role,
                        JoinedAt = DateTime.UtcNow.AddDays(-_random.Next(1, 60)),
                        InvitedById = org.CreatedById
                    });
                }
            }

            // Evita duplicações pela unique (UserId, OrganizationId)
            // Mas como a tabela está vazia, podemos inserir diretamente.
            await _db.OrganizationMembers.AddRangeAsync(memberships);
            await _db.SaveChangesAsync();
        }

        private async Task SeedGroupsAsync()
        {
            if (await _db.Groups.AnyAsync()) return;

            var orgs = await _db.Organizations.ToListAsync();
            var creators = await _db.Users.ToListAsync();
            var groups = new List<Group>();
            var colors = new[]
            {
                "#E07A5F", "#81B29A", "#3D405B", "#c27c0c", "#A8DADC", "#8b45d1", "#63a889"
            };
            var icons = new[] { "calendar", "users", "bell", "star", "church" };

            foreach (var org in orgs)
            {
                for (int i = 0; i < GroupsPerOrganization; i++)
                {
                    var creator = creators[_random.Next(creators.Count)];
                    groups.Add(new Group
                    {
                        OrganizationId = org.Id,
                        CreatedById = creator.Id,
                        Name = $"Grupo {i + 1} da {org.Name}",
                        Color = colors[_random.Next(colors.Length)],
                        Icon = icons[_random.Next(icons.Length)]
                    });
                }
            }

            await _db.Groups.AddRangeAsync(groups);
            await _db.SaveChangesAsync();
        }

        private async Task SeedPositionsAsync()
        {
            if (await _db.Positions.AnyAsync()) return;

            var groups = await _db.Groups.ToListAsync();
            var positions = new List<Position>();
            var posNames = new[] { "Recepção", "Segurança", "Logística", "Atendimento", "Coordenação", "Suporte", "Backoffice", "Técnico" };

            foreach (var group in groups)
            {
                for (int i = 0; i < PositionsPerGroup; i++)
                {
                    positions.Add(new Position
                    {
                        GroupId = group.Id,
                        Name = posNames[_random.Next(posNames.Length)]
                    });
                }
            }

            await _db.Positions.AddRangeAsync(positions);
            await _db.SaveChangesAsync();
        }

        private async Task SeedShiftsAsync()
        {
            if (await _db.Shifts.AnyAsync()) return;

            var groups = await _db.Groups.ToListAsync();
            var shifts = new List<Shift>();

            foreach (var group in groups)
            {
                for (int i = 0; i < ShiftsPerGroup; i++)
                {
                    // Próximos meses: entre 1 e 3 meses adiante
                    var monthsAhead = _random.Next(1, 4);
                    var day = _random.Next(1, 28);
                    var startLocal = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1)
                        .AddMonths(monthsAhead)
                        .AddDays(day - 1)
                        .AddHours(_random.Next(8, 18));
                    var start = EnsureUtc(startLocal);
                    var end = EnsureUtc(startLocal.AddHours(_random.Next(4, 8)));

                    shifts.Add(new Shift
                    {
                        GroupId = group.Id,
                        StartDate = start,
                        EndDate = end,
                        Title = $"Turno #{i + 1} - {group.Name}",
                        Notes = "Escala gerada automaticamente pelo seeder.",
                        Status = ShiftStatusEnum.OPEN
                    });
                }
            }

            await _db.Shifts.AddRangeAsync(shifts);
            await _db.SaveChangesAsync();
        }

        private async Task SeedShiftPositionsAsync()
        {
            if (await _db.ShiftPositions.AnyAsync()) return;

            var shifts = await _db.Shifts.ToListAsync();
            var positionsByGroup = await _db.Positions.GroupBy(p => p.GroupId).ToDictionaryAsync(g => g.Key, g => g.ToList());
            var shiftPositions = new List<ShiftPosition>();

            foreach (var shift in shifts)
            {
                if (!positionsByGroup.TryGetValue(shift.GroupId, out var positions)) continue;

                // Para cada shift, cria 2-4 posições de escala
                foreach (var position in positions.OrderBy(_ => _random.Next()).Take(_random.Next(2, 5)))
                {
                    var required = _random.Next(1, 4);
                    shiftPositions.Add(new ShiftPosition
                    {
                        ShiftId = shift.Id,
                        PositionId = position.Id,
                        RequiredCount = required,
                        VolunteersCount = 0
                    });
                }
            }

            await _db.ShiftPositions.AddRangeAsync(shiftPositions);
            await _db.SaveChangesAsync();
        }

        private async Task SeedVolunteersAsync()
        {
            if (await _db.ShiftPositionAssignments.AnyAsync()) return;

            var usersByOrg = await _db.OrganizationMembers
                .GroupBy(m => m.OrganizationId)
                .ToDictionaryAsync(g => g.Key, g => g.Select(x => x.UserId).Distinct().ToList());

            var shiftPositions = await _db.ShiftPositions
                .Include(sp => sp.Shift)
                .Include(sp => sp.Position)
                .ToListAsync();

            var assignments = new List<ShiftPositionAssignment>();

            foreach (var sp in shiftPositions)
            {
                // Usuários válidos: os membros da organização do grupo da posição
                var orgId = (await _db.Groups.Where(g => g.Id == sp.Position.GroupId).Select(g => g.OrganizationId).FirstAsync());
                if (!usersByOrg.TryGetValue(orgId, out var userIds) || userIds.Count == 0) continue;

                var maxVolunteers = _random.Next(1, VolunteersPerShiftPositionMax + 1);
                foreach (var userId in userIds.OrderBy(_ => _random.Next()).Take(maxVolunteers))
                {
                    var statusDraw = _random.Next(0, 100);
                    var status = statusDraw < 60 ? VolunteerStatusEnum.CONFIRMED
                                 : statusDraw < 85 ? VolunteerStatusEnum.PENDING
                                 : VolunteerStatusEnum.CANCELLED;

                    assignments.Add(new ShiftPositionAssignment
                    {
                        UserId = userId,
                        ShiftPositionId = sp.Id,
                        Status = status,
                        Notes = status == VolunteerStatusEnum.CANCELLED ? "Cancelado pelo voluntário" : null,
                        AppliedAt = EnsureUtc(sp.Shift.StartDate.AddDays(-_random.Next(5, 20))),
                        ConfirmedAt = status == VolunteerStatusEnum.CONFIRMED ? EnsureUtc(sp.Shift.StartDate.AddDays(-_random.Next(1, 4))) : null,
                        RejectedAt = status == VolunteerStatusEnum.CANCELLED ? EnsureUtc(sp.Shift.StartDate.AddDays(-_random.Next(1, 4))) : null
                    });
                }
            }

            await _db.ShiftPositionAssignments.AddRangeAsync(assignments);
            await _db.SaveChangesAsync();
        }
    }
}