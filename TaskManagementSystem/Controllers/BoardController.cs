using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Data;
using TaskManagementSystem.Models;

namespace TaskManagementSystem.Controllers
{
    [Authorize]
    public class BoardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BoardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var boards = await _context.Boards
                .Where(b => b.OwnerId == user.Id)
                .ToListAsync();
            return View(boards);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) return RedirectToAction(nameof(Index));

            var user = await _userManager.GetUserAsync(User);
            var board = new Board
            {
                Title = title,
                OwnerId = user.Id,
                BackgroundColor = "#1E293B" // Default dark color
            };

            // Add default lists
            board.Lists.Add(new BoardList { Title = "To Do", Order = 1 });
            board.Lists.Add(new BoardList { Title = "In Progress", Order = 2 });
            board.Lists.Add(new BoardList { Title = "Done", Order = 3 });

            _context.Boards.Add(board);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = board.Id });
        }

        public async Task<IActionResult> Details(int id)
        {
            var board = await _context.Boards
                .Include(b => b.Lists.OrderBy(l => l.Order))
                    .ThenInclude(l => l.Cards.OrderBy(c => c.Order))
                .FirstOrDefaultAsync(b => b.Id == id);

            if (board == null) return NotFound();

            return View(board);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCard(int listId, string title)
        {
            if (string.IsNullOrWhiteSpace(title)) return BadRequest();

            var list = await _context.BoardLists.Include(l => l.Cards).FirstOrDefaultAsync(l => l.Id == listId);
            if (list == null) return NotFound();

            var newOrder = list.Cards.Count > 0 ? list.Cards.Max(c => c.Order) + 1 : 1;
            
            var card = new Card
            {
                Title = title,
                BoardListId = listId,
                Order = newOrder
            };

            _context.Cards.Add(card);
            await _context.SaveChangesAsync();

            return Ok(card);
        }

        [HttpPost]
        public async Task<IActionResult> MoveCard(int cardId, int newListId, int newOrder)
        {
            var card = await _context.Cards.FindAsync(cardId);
            if (card == null) return NotFound();

            if (card.BoardListId != newListId)
            {
                // Moved to a different list
                var oldListCards = await _context.Cards.Where(c => c.BoardListId == card.BoardListId && c.Order > card.Order).ToListAsync();
                foreach(var c in oldListCards) c.Order--;

                var newListCards = await _context.Cards.Where(c => c.BoardListId == newListId && c.Order >= newOrder).ToListAsync();
                foreach(var c in newListCards) c.Order++;

                card.BoardListId = newListId;
                card.Order = newOrder;
            }
            else
            {
                // Moved within the same list
                var listCards = await _context.Cards.Where(c => c.BoardListId == card.BoardListId).OrderBy(c => c.Order).ToListAsync();
                listCards.Remove(card);
                listCards.Insert(newOrder - 1, card);

                for (int i = 0; i < listCards.Count; i++)
                {
                    listCards[i].Order = i + 1;
                }
            }

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
