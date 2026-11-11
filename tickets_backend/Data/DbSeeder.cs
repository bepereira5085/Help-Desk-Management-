using TicketsBackend.Enums;
using TicketsBackend.Models;

namespace TicketsBackend.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (!db.Departments.Any())
        {
            db.Departments.AddRange(
                new Department { Name = "TI", Color = "#1976D2" },
                new Department { Name = "RH", Color = "#9C27B0" },
                new Department { Name = "Financeiro", Color = "#F57C00" }
            );
            db.SaveChanges();
        }

        if (!db.Tags.Any())
        {
            db.Tags.AddRange(
                new Tag { Name = "Wifi", Color = "#00ACC1" },
                new Tag { Name = "Computador", Color = "#5C6BC0" },
                new Tag { Name = "Periféricos", Color = "#8D6E63" }
            );
            db.SaveChanges();
        }

        SeedUsers(db);

        SeedDemoTickets(db, 100);
    }

    // Usuários iniciais: (login, senha, nome, setor, é admin)
    private static readonly (string Login, string Password, string Name, string Department, bool IsAdmin)[] SeedUserList =
    {
        ("admin", "admin123", "Administrador", "TI", true),
        ("usuario", "usuario123", "Usuário Comum", "RH", false),
        ("maria", "maria123", "Maria Souza", "Financeiro", false),
        ("joao", "joao123", "João Pereira", "TI", false),
        ("ana", "ana123", "Ana Lima", "RH", false)
    };

    /// <summary>Cria os usuários iniciais que ainda não existem (verifica pelo login).</summary>
    private static void SeedUsers(AppDbContext db)
    {
        var existingLogins = db.Users.Select(u => u.Login).ToHashSet();
        var departments = db.Departments.ToList();

        foreach (var seed in SeedUserList.Where(u => !existingLogins.Contains(u.Login)))
        {
            var department = departments.FirstOrDefault(d => d.Name == seed.Department) ?? departments.First();
            db.Users.Add(new User
            {
                Login = seed.Login,
                Name = seed.Name,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(seed.Password),
                DepartmentId = department.Id,
                IsAdmin = seed.IsAdmin
            });
        }

        db.SaveChanges();
    }

    // Exemplos de itens: (nome do item, descrições possíveis, tags relacionadas)
    private static readonly (string Item, string[] Descriptions, string[] Tags)[] DemoItems =
    {
        ("Computador", new[]
        {
            "O computador não liga. A luz da fonte pisca, mas não dá vídeo.",
            "Computador muito lento ao abrir qualquer programa.",
            "Tela azul aparece várias vezes ao dia.",
            "Computador reinicia sozinho durante o uso."
        }, new[] { "Computador" }),
        ("Notebook", new[]
        {
            "A bateria do notebook não segura carga por mais de 20 minutos.",
            "Notebook esquentando muito e desligando.",
            "Tecla Enter do notebook parou de funcionar."
        }, new[] { "Computador", "Periféricos" }),
        ("Impressora", new[]
        {
            "Impressora não está imprimindo, fica na fila.",
            "Impressão saindo com manchas e falhas.",
            "Papel atolando com frequência.",
            "Impressora não aparece na rede."
        }, new[] { "Periféricos" }),
        ("Monitor", new[]
        {
            "Monitor piscando e com listras na tela.",
            "Monitor não liga, sem sinal de energia."
        }, new[] { "Periféricos" }),
        ("Teclado", new[]
        {
            "Teclado parou de funcionar de repente.",
            "Algumas teclas não respondem."
        }, new[] { "Periféricos" }),
        ("Mouse", new[]
        {
            "Mouse com clique duplo involuntário.",
            "Cursor do mouse travando na tela."
        }, new[] { "Periféricos" }),
        ("Roteador", new[]
        {
            "Wifi caindo a cada poucos minutos.",
            "Sinal de wifi muito fraco na sala.",
            "Sem acesso à internet no setor."
        }, new[] { "Wifi" }),
        ("Sistema ERP", new[]
        {
            "Erro ao gerar relatório mensal no ERP.",
            "Não consigo acessar o ERP, senha bloqueada.",
            "ERP muito lento para salvar pedidos."
        }, new[] { "Computador" }),
        ("E-mail", new[]
        {
            "Não estou recebendo e-mails externos.",
            "Caixa de e-mail cheia, preciso de mais espaço.",
            "Outlook não sincroniza."
        }, new[] { "Wifi", "Computador" }),
        ("Telefone IP", new[]
        {
            "Telefone sem linha.",
            "Chamadas caindo após alguns segundos."
        }, new[] { "Wifi" }),
        ("Projetor", new[]
        {
            "Projetor da sala de reuniões não reconhece o notebook.",
            "Lâmpada do projetor queimada."
        }, new[] { "Periféricos" }),
        ("Scanner", new[]
        {
            "Scanner não é reconhecido pelo computador.",
            "Digitalização saindo cortada."
        }, new[] { "Periféricos", "Computador" })
    };

    /// <summary>Gera chamados de exemplo até o banco ter pelo menos <paramref name="minimum"/> chamados.</summary>
    private static void SeedDemoTickets(AppDbContext db, int minimum)
    {
        var count = minimum - db.Tickets.Count();
        if (count <= 0) return;

        var random = new Random(2026); // semente fixa: sempre gera os mesmos dados
        // Os chamados de exemplo são distribuídos entre os usuários comuns
        var users = db.Users.Where(u => !u.IsAdmin).ToList();
        if (users.Count == 0) users = db.Users.ToList();
        var tagsByName = db.Tags.ToDictionary(t => t.Name, t => t.Id);
        var now = DateTime.Now;

        var tickets = new List<Ticket>();
        for (var i = 0; i < count; i++)
        {
            var demo = DemoItems[random.Next(DemoItems.Length)];

            // Chamados mais antigos tendem a já estar finalizados
            var createdAt = now
                .AddDays(-random.Next(0, 60))
                .AddHours(-random.Next(0, 24))
                .AddMinutes(-random.Next(0, 60));
            var ageDays = (now - createdAt).TotalDays;
            var roll = random.NextDouble();
            var status = ageDays > 30
                ? (roll < 0.75 ? TicketStatus.Finalizado : roll < 0.9 ? TicketStatus.EmAndamento : TicketStatus.Aberto)
                : (roll < 0.45 ? TicketStatus.Aberto : roll < 0.75 ? TicketStatus.EmAndamento : TicketStatus.Finalizado);

            var tagIds = demo.Tags
                .Where(name => tagsByName.ContainsKey(name) && random.NextDouble() < 0.8)
                .Select(name => tagsByName[name])
                .ToList();

            var user = users[i % users.Count]; // reparte igualmente entre eles

            tickets.Add(new Ticket
            {
                UserId = user.Id,
                DepartmentId = user.DepartmentId, // setor de origem de quem abriu
                CreatedAt = createdAt,
                ItemName = demo.Item,
                Description = demo.Descriptions[random.Next(demo.Descriptions.Length)],
                Status = status,
                Priority = (TicketPriority)random.Next(1, 5),
                EquipmentTag = random.NextDouble() < 0.6 ? $"PAT-{random.Next(1, 9999):D4}" : null,
                TicketTags = tagIds.Select(id => new TicketTag { TagId = id }).ToList()
            });
        }

        db.Tickets.AddRange(tickets);
        db.SaveChanges();
    }
}
