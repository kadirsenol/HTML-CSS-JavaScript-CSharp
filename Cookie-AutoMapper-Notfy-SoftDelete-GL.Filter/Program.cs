using AspNetCoreHero.ToastNotification.Extensions;
using Cookie_AutoMapper_Notfy_SoftDelete_GL.Filter.ExtensionsandConfig.AutoMapper;
using Cookie_AutoMapper_Notfy_SoftDelete_GL.Filter.ExtensionsandConfig.Services;
using Serilog;

namespace Cookie_AutoMapper_Notfy_SoftDelete_GL.Filter
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            #region Logger
            Log.Logger = new LoggerConfiguration() // Siteyi local serverinde yayinlayacaksan burayi aktiflestirebilirsin, host satin alacaksan loggeri ve try cath finaly k�s�mlarini kaldir.
                                 .WriteTo.Console() // Console bas
                                 .WriteTo.Seq(@"http://localhost:5201") // Nereyi dinlicek. Seq i�erisinde listenin ip ise programin kendisinin nerede calisacagini ayarliyorsun
                                 .CreateLogger(); // Logger i olustur 
            #endregion
            try
            {
                Log.Information("Sistem calismaya basladi");

                var builder = WebApplication.CreateBuilder(args);

                // Add services to the container.
                builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();//AddRazor extension metodu; anlik degisimleri
                                                                                        //refresh esnasinda hemen yansitiyor. Bunun icin Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation paketini indir.

                #region Tek Satirlik Servis Eklentileri
                #region DbContext
                /*builder.Services.AddDbContext<SqlDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("MyConstr")));*/ // Normalde dbcontext icerisinde ki constr efnin cli kullanimi icindir. bu nedenle constr yi servis icin ayr� belirtmek durumundayiz.
                                                                                                                                                         // Guvenlik acisindan(Db ve server ismi) constr yi secrets dosyam�za gomduk ve "MyConstr" ile oray� refere ediyoruz.                                                                                                                                                    
                                                                                                                                                         // Ama bu projede herhangi bir ctora bagimli olarak dbcontext vermedigim icin bunu comment isaretliyorum. 
                                                                                                                                                         // Servislere ilk bag�ml� nesneyi verdikten sonra ilgili nesnenin bag�ml�l�klar�n� kendimiz newletebiliriz cunku bi kere ilk basta servis newleme islemini baslatti.
                #endregion
                #region AutoMapper
                builder.Services.AddAutoMapper(typeof(AutoMapperConfig));
                #endregion
                #endregion

                #region Cok Satirlik Servis Eklentileri
                #region ExtensionsandConfig
                builder.Services.AddNotyfSetting();
                builder.Services.AddCookieSetting();
                builder.Services.AddScopedAll();
                builder.Services.AddSpecialPolicy(); // TCNO adinda bir policy olusturdum ve bunu TcNo adinda ki claim im ile birlestirdim. Ve policy olustururken iceriginin 123 olmasini istedim. Bunu [Authorize(Policy ="TCNO")] ile dene.
                #endregion
                #endregion

                var app = builder.Build();

                // Configure the HTTP request pipeline.
                if (!app.Environment.IsDevelopment())
                {
                    app.UseExceptionHandler("/Home/Error");
                    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                    app.UseHsts();
                }

                app.UseNotyf();

                app.UseHttpsRedirection();
                app.UseStaticFiles();

                app.UseRouting();

                app.UseAuthentication(); //Authentication ozelligini kullanmasi icin ekliyoruz. Ve UseAuthorization metodundan once olmas� gerek.
                app.UseAuthorization();

                app.UseEndpoints(endpoints => //Admin alani icin, varsa router i ekliyoruz.
                {
                    endpoints.MapControllerRoute(
                      name: "areas",
                      pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
                    );
                });

                app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                app.Run();
            }
            catch (Exception ex)
            {

                Log.Fatal(ex, "Sistem beklenmedik bir nedenden dolay� durdu."); // Alinan tum exceptionlar fatal ile seqe gonderilecek
            }
            finally
            {
                Log.CloseAndFlush(); // Hem kapatsin hemde elinde ne kadar tas varsa hepsini seq e gondersin
            }
        }
    }
}
