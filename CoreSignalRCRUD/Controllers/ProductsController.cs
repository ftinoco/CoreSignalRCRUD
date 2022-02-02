using CoreSignalRCRUD.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CoreSignalRCRUD.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDBContext _context;
        private readonly IHubContext<SignalRServer> _signalrHub;

        public ProductsController(
            ApplicationDBContext context,
            IHubContext<SignalRServer> signalrHub
        )
        {
            _context = context;
            _signalrHub = signalrHub;
        }

        // GET: ProductsController
        public async Task<ActionResult> Index()
        {
            return View(await _context.Products.ToListAsync());
        }

        [HttpGet]
        public IActionResult GetProducts()
        {
            var res = _context.Products.ToList();
            return Ok(res);
        }

        // GET: ProductsController/Details/5
        public async Task<ActionResult> DetailsAsync(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FirstOrDefaultAsync(x => x.ProdId == id);

            if (product == null) return NotFound();

            return View(product);
        }

        // GET: ProductsController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ProductsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateAsync([Bind("ProdId,ProdName,Category,UnitPrice,StockQty")] Products product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                await _signalrHub.Clients.All.SendAsync("LoadProducts");
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: ProductsController/Edit/5
        public async Task<ActionResult> EditAsync(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FindAsync(id);

            if (product == null) return NotFound();

            return View(product);
        }

        // POST: ProductsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAsync(int ProdId, [Bind("ProdId,ProdName,Category,UnitPrice,StockQty")] Products product)
        {
            if (ProdId != product.ProdId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                    await _signalrHub.Clients.All.SendAsync("LoadProducts");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProdId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: ProductsController/Delete/5
        public async Task<ActionResult> DeleteAsync(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FirstOrDefaultAsync(x => x.ProdId == id);

            if (product == null) return NotFound();

            return View(product);
        }

        // POST: ProductsController/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteAsync(int ProdId)
        {
            var product = await _context.Products.FindAsync(ProdId);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync(); 
            await _signalrHub.Clients.All.SendAsync("LoadProducts");
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(x => x.ProdId == id);
        }
    }
}
