using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TwoFactorAuth;
using TwoFactorAuth.App_Code;
using SourceAFIS.Simple;
using System.Drawing;
using System.Xml.Linq;

namespace TwoFactorAuth.Controllers
{
    public class logsController : Controller
    {
        private fingerprintidentificationsystemEntities db = new fingerprintidentificationsystemEntities();

        // GET: logs
        public async Task<ActionResult> Index()
        {
            var logs = db.logs.Include(l => l.mortal);
            return View(await logs.ToListAsync());
        }

        // GET: logs/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            log log = await db.logs.FindAsync(id);
            if (log == null)
            {
                return HttpNotFound();
            }
            return View(log);
        }

        // GET: logs/Create
        public ActionResult Create()
        {
            ViewBag.mortalId = new SelectList(db.mortals, "mortalId", "name");
            return View();
        }

        // POST: logs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "logId,mortalId,dateTime_2,areaCode")] log log, FormCollection collection)
        {
            string fileNameInput = collection["filename"];

            AfisEngine Afis = new AfisEngine();
            Afis.Threshold = 10;

            Fingerprint fp1 = new Fingerprint();

            string apppath = System.IO.Path.Combine(Server.MapPath("~"), "images");
            string pathToImage = (System.IO.Path.Combine(apppath, fileNameInput));

            Bitmap bitmap = fp1.AsBitmap = new Bitmap(Image.FromFile(pathToImage));

            MyPerson personsdk = new MyPerson();
            personsdk.Fingerprints.Add(fp1);

            Afis.Extract(personsdk);

            string sql = "Select * from mortal";
            List<mortal> personListRom = db.mortals.SqlQuery(sql).ToList();
            List<MyPerson> personListRam = new List<MyPerson>();
            foreach (mortal p in personListRom)
            {

                MyPerson personTemp = new MyPerson();
                MyFingerprint fpTemp = new MyFingerprint();
                personTemp.Id = p.mortalId;
                personTemp.Name = p.name;
                fpTemp.Filename = p.filename;
                fpTemp.AsXmlTemplate = XElement.Parse(p.template);
                personTemp.Fingerprints.Add(fpTemp);
                personListRam.Add(personTemp);
            }

            MyPerson match = Afis.Identify(personsdk, personListRam).FirstOrDefault() as MyPerson;
            log.mortalId = match.Id;
            if (ModelState.IsValid)
            {
                db.logs.Add(log);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.mortalId = new SelectList(db.mortals, "mortalId", "name", log.mortalId);
            return View(log);
        }

        // GET: logs/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            log log = await db.logs.FindAsync(id);
            if (log == null)
            {
                return HttpNotFound();
            }
            ViewBag.mortalId = new SelectList(db.mortals, "mortalId", "name", log.mortalId);
            return View(log);
        }

        // POST: logs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "logId,mortalId,dateTime_2,areaCode")] log log)
        {
            if (ModelState.IsValid)
            {
                db.Entry(log).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.mortalId = new SelectList(db.mortals, "mortalId", "name", log.mortalId);
            return View(log);
        }

        // GET: logs/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            log log = await db.logs.FindAsync(id);
            if (log == null)
            {
                return HttpNotFound();
            }
            return View(log);
        }

        // POST: logs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            log log = await db.logs.FindAsync(id);
            db.logs.Remove(log);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
