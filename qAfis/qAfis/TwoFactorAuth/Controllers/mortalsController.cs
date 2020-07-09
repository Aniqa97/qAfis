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
using System.Diagnostics;
using Jitbit.Utils;

namespace TwoFactorAuth.Controllers
{
    public class mortalsController : Controller
    {
        private fingerprintidentificationsystemEntities db = new fingerprintidentificationsystemEntities();

        // GET: mortals
        public async Task<ActionResult> Index()
        {
            return View(await db.mortals.ToListAsync());
        }

        // GET: mortals/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            mortal mortal = await db.mortals.FindAsync(id);
            if (mortal == null)
            {
                return HttpNotFound();
            }
            return View(mortal);
        }

        // GET: mortals/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: mortals/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "mortalId,name,nic,filename,template,no_of_minutaes")] mortal mortal)
        {
            if (ModelState.IsValid & mortal.filename != null)
            {

                AfisEngine Afis = new AfisEngine();

                Fingerprint fp1 = new Fingerprint();

                string AppPath = System.IO.Path.Combine(Server.MapPath("~"), "images");
                string pathToImage = (System.IO.Path.Combine(AppPath, mortal.filename));

                Bitmap bitmap = fp1.AsBitmap = new Bitmap(Image.FromFile(pathToImage));

                MyPerson personsdk = new MyPerson();
                personsdk.Fingerprints.Add(fp1);

                Afis.Extract(personsdk);
                string str_Template = fp1.AsXmlTemplate.ToString();
                mortal.template = str_Template;

                //Code to count number of Minutae
                string pattern = "<Minutia";
                int count = 0;
                int a = 0;
                while ((a = str_Template.IndexOf(pattern, a)) != -1)
                {
                    a += pattern.Length;
                    count++;
                }
                //Assign count to no_of_minutaes
                mortal.no_of_minutaes = count;
                db.mortals.Add(mortal);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            Response.Write("<script>alert('Please Enter Filename');</script>");
            return View(mortal);
        }

        // GET: mortals/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            mortal mortal = await db.mortals.FindAsync(id);
            if (mortal == null)
            {
                return HttpNotFound();
            }
            return View(mortal);
        }

        // POST: mortals/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "mortalId,name,nic,filename,template,no_of_minutaes")] mortal mortal)
        {
            if (ModelState.IsValid)
            {
                db.Entry(mortal).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(mortal);
        }

        // GET: mortals/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            mortal mortal = await db.mortals.FindAsync(id);
            if (mortal == null)
            {
                return HttpNotFound();
            }
            return View(mortal);
        }

        // POST: mortals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            mortal mortal = await db.mortals.FindAsync(id);
            db.mortals.Remove(mortal);
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


        //GET: mortals/Verify
        public ActionResult Verify()
        {
            return View();
        }

        //POST: mortals/Verify
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Verify([Bind(Include = "Filename")] mortal mortal)
        {
            if (ModelState.IsValid & mortal.filename != null)
            {
                AfisEngine Afis = new AfisEngine();
                Afis.Threshold = 10;

                Fingerprint fp1 = new Fingerprint();

                string apppath = System.IO.Path.Combine(Server.MapPath("~"), "images");
                string pathToImage = (System.IO.Path.Combine(apppath, mortal.filename));

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
                Response.Write("<script>alert('" + match.Name + "');</script>");
                return View();
            }
            Response.Write("<script>alert('Please Enter Filename');</script>");
            return View();
        }

        //GET: mortals/Match
        public ActionResult Match()
        {
            return View();
        }

        //POST: mortals/Match
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Match([Bind(Include = "Name, Filename")] mortal mortal)
        {
            if (ModelState.IsValid & mortal.filename != null)
            {
                AfisEngine Afis = new AfisEngine();
                Afis.Threshold = 10;

                Fingerprint fp1 = new Fingerprint();
                Fingerprint fp2 = new Fingerprint();

                MyPerson person1 = new MyPerson();
                MyPerson person2 = new MyPerson();

                string apppath = System.IO.Path.Combine(Server.MapPath("~"), "images");
                string pathToImage1 = (System.IO.Path.Combine(apppath, mortal.name));
                string pathToImage2 = (System.IO.Path.Combine(apppath, mortal.filename));


                Bitmap bitmap1 = fp1.AsBitmap = new Bitmap(Image.FromFile(pathToImage1));
                Bitmap bitmap2 = fp2.AsBitmap = new Bitmap(Image.FromFile(pathToImage2));


                person1.Fingerprints.Add(fp1);
                person2.Fingerprints.Add(fp2);
                Afis.Extract(person1);
                Afis.Extract(person2);
                float score = Afis.Verify(person1, person2);
                if (score > Afis.Threshold)

                    Response.Write("<script>alert('" + score.ToString() + "');</script>");
                return View();
            }
            Response.Write("<script>alert('Please Enter Filename');</script>");
            return View();
        }

        //GET: mortals/BCreate
        //For Bulk Enrollment and Verification Times
        public ActionResult BulkCreate()
        {
            return View();
        }

        // POST: mortals/BCreate
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> BulkCreate([Bind(Include = "")] mortal mortal/*, HttpPostedFileBase file*/)
        {
            AfisEngine Afis = new AfisEngine();
            Afis.Threshold = 60;
            //List<MyPerson> personsdks = new List<MyPerson>();
            string AppPath = System.IO.Path.Combine(Server.MapPath("~"), "images");
            Stopwatch stopwatch0 = Stopwatch.StartNew(); //creates and start the instance of Stopwatch
            long range = Int64.Parse(mortal.nic);
            for (int i = 0; i < range; i++)
            {
                int imgnumber = i + 1;
                string imgval = "(" + imgnumber.ToString() + ").jpg";
                string pathtoimages = (System.IO.Path.Combine(AppPath, imgval));
                Fingerprint fp0 = new Fingerprint();
                MyPerson myperson = new MyPerson();
                Bitmap bitmap0 = fp0.AsBitmap = new Bitmap(Image.FromFile(pathtoimages));
                myperson.Fingerprints.Add(fp0);
                Afis.Extract(myperson);
                string str_Template = fp0.AsXmlTemplate.ToString();
                mortal.template = str_Template;
                mortal.name = (i + 1).ToString();
                mortal.filename = pathtoimages;
                mortal.nic = (i + 1).ToString();

                //Code to count number of Minutae
                string pattern = "<Minutia";
                int count = 0;
                int a = 0;
                while ((a = str_Template.IndexOf(pattern, a)) != -1)
                {
                    a += pattern.Length;
                    count++;
                }

                mortal.no_of_minutaes = count;

                db.mortals.Add(mortal);
                //db.Entry(person).State = EntityState.Modified;
                await db.SaveChangesAsync();


            }

            stopwatch0.Stop();
            Stopwatch stopwatch1 = Stopwatch.StartNew(); //creates and start the instance of Stopwatch

            //Verification segment




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
            stopwatch1.Stop();
            //verify at 1 index
            Stopwatch stopwatch2 = Stopwatch.StartNew(); //creates and start the instance of Stopwatch

            Fingerprint fp1 = new Fingerprint();

            string apppath = System.IO.Path.Combine(Server.MapPath("~"), "images");
            string pathToImage = (System.IO.Path.Combine(apppath, "(1).jpg"));

            Bitmap bitmap = fp1.AsBitmap = new Bitmap(Image.FromFile(pathToImage));

            MyPerson personsdk = new MyPerson();
            personsdk.Fingerprints.Add(fp1);
            Afis.Extract(personsdk);

            MyPerson match = Afis.Identify(personsdk, personListRam).FirstOrDefault() as MyPerson;
            stopwatch2.Stop();

            //verify at mid index
            Stopwatch stopwatch3 = Stopwatch.StartNew(); //creates and start the instance of Stopwatch

            Fingerprint fp2 = new Fingerprint();

            string apppath2 = System.IO.Path.Combine(Server.MapPath("~"), "images");
            string pathToImage2 = (System.IO.Path.Combine(apppath, "(" + (range / 2).ToString() + ").jpg"));

            Bitmap bitmap2 = fp2.AsBitmap = new Bitmap(Image.FromFile(pathToImage2));

            MyPerson personsdk2 = new MyPerson();
            personsdk2.Fingerprints.Add(fp2);
            Afis.Extract(personsdk2);

            MyPerson match2 = Afis.Identify(personsdk2, personListRam).FirstOrDefault() as MyPerson;
            stopwatch3.Stop();

            Stopwatch stopwatch4 = Stopwatch.StartNew(); //creates and start the instance of Stopwatch

            Fingerprint fp3 = new Fingerprint();
            Random rand = new Random();
            int num = rand.Next(3);
            string apppath3 = System.IO.Path.Combine(Server.MapPath("~"), "images");
            string pathToImage3 = (System.IO.Path.Combine(apppath, "(" + num.ToString() + ").jpg"));

            Bitmap bitmap3 = fp3.AsBitmap = new Bitmap(Image.FromFile(pathToImage3));

            MyPerson personsdk3 = new MyPerson();
            personsdk3.Fingerprints.Add(fp3);
            Afis.Extract(personsdk3);

            MyPerson match3 = Afis.Identify(personsdk3, personListRam).FirstOrDefault() as MyPerson;
            stopwatch4.Stop();

            string stopwatchtime0 = stopwatch0.ElapsedMilliseconds.ToString();
            string stopwatchtime1 = stopwatch1.ElapsedMilliseconds.ToString();
            string stopwatchtime2 = stopwatch2.ElapsedMilliseconds.ToString();
            string stopwatchtime3 = stopwatch3.ElapsedMilliseconds.ToString();
            string stopwatchtime4 = stopwatch4.ElapsedMilliseconds.ToString();


            Response.Write("<script>alert('Enrollment time in database is:" + stopwatchtime0 + "');</script>");
            Response.Write("<script>alert('Enrollment time in ram is:" + stopwatchtime1 + "');</script>");
            Response.Write("<script>alert('Verification time for index 1:" + stopwatchtime2 + "');</script>");
            Response.Write("<script>alert('Verification time for index mid:" + stopwatchtime3 + "');</script>");
            Response.Write("<script>alert('Verification time for index last:" + stopwatchtime4 + "');</script>");

            return View(mortal);
        }

        //GET: mortals/MCreate
        //For verification with manipulated fingerprints
        public ActionResult ManCreate()
        {
            return View();
        }

        //POST: mortals/MCreate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ManCreate([Bind(Include = "Name, Filename")] mortal mortal)
        {
            AfisEngine Afis = new AfisEngine();
            Afis.Threshold = 180;

            var myExport = new CsvExport();
            string apppath = System.IO.Path.Combine(Server.MapPath("~"), "images");


            for (int i = 0; i < 40; i++)
            {

                string pathToImageO = (System.IO.Path.Combine(apppath, "original/" + (i + 1).ToString() + ".png"));

                for (int j = 0; j < 40; j++)
                {
                    string pathToImageM2 = (System.IO.Path.Combine(apppath, "augmented/" + (j + 1).ToString() + ".jpg"));
                    Fingerprint fp1 = new Fingerprint();
                    Fingerprint fp2 = new Fingerprint();

                    MyPerson personO = new MyPerson();
                    MyPerson personM = new MyPerson();

                    Bitmap bitmap1 = fp1.AsBitmap = new Bitmap(Image.FromFile(pathToImageO));
                    Bitmap bitmap2 = fp2.AsBitmap = new Bitmap(Image.FromFile(pathToImageM2));

                    personO.Fingerprints.Add(fp1);
                    personM.Fingerprints.Add(fp2);
                    Afis.Extract(personO);
                    Afis.Extract(personM);
                    float score = Afis.Verify(personO, personM);
                    if (score > Afis.Threshold)
                    {
                        myExport.AddRow();
                        myExport["Theshold"] = Afis.Threshold;
                        myExport["Match"] = 1;
                        myExport["FingerprintO"] = i + 1;
                        myExport["FingerprintM"] = j + 1;
                    }
                    else
                    {
                        myExport.AddRow();
                        myExport["Theshold"] = Afis.Threshold;
                        myExport["Match"] = 0;
                        myExport["FingerprintO"] = i + 1;
                        myExport["FingerprintM"] = j + 1;

                    }
                }











            }






            ///ASP.NET MVC action example
            return File(myExport.ExportToBytes(), "text/csv", "results(180).csv");

            //return View();
            //Response.Write("<script>alert('Please Enter Filename');</script>");
        }

    }
}
