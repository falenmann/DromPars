using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;

namespace auto.drom
{
    public class CarModel
    {
        public string Model { get; set; }
        public List<string> Generations { get; set; }
    }

    public class CarMake
    {
        public string Make { get; set; }
        public List<CarModel> Models { get; set; }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            List<string> completedMakes = new List<string>();
            var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "manufacturer_*.json");
            foreach (var file in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                var makeName = fileName.Replace("manufacturer_", "");
                completedMakes.Add(makeName);
            }

            IWebDriver driver = new ChromeDriver();
            
            try
            {
                driver.Navigate().GoToUrl("https://www.drom.ru/catalog/");
                Thread.Sleep(1000);

                IWebElement openAll = driver.FindElement(By.CssSelector(".css-1qm0fmp.e1lm3vns0"));
                openAll.Click();
                
                
                ReadOnlyCollection<IWebElement> openAll_2 = driver.FindElements(By.CssSelector(".css-u4n5gw.e4ojbx41"));

                Actions actions = new Actions(driver);
                
                foreach (var mark in openAll_2)
                {
                    IWebElement openAll_3 = mark.FindElement(By.CssSelector(".css-1q66we5.e4ojbx43"));
                    string markName = openAll_3.Text;
                    
                    if (completedMakes.Contains(markName))
                    {
                        continue;
                    }
                    actions.MoveToElement(openAll_3)
                        .KeyDown(Keys.Control)
                        .Click(openAll_3)
                        .KeyUp(Keys.Control)
                        .Perform();
                    Thread.Sleep(1000);
                    
                    var tabs = driver.WindowHandles;

                int count = 0;
                
                    count++;
                    
                    string currentWindowHandle = driver.CurrentWindowHandle;
                    IList<string> windowHandles = driver.WindowHandles;
                    
                    string newTabHandle = windowHandles[windowHandles.Count - 1];
                    driver.SwitchTo().Window(newTabHandle);

                                try
                                {
                                    string currentUrl = driver.Url;
                                    /*ReadOnlyCollection<IWebElement> models = driver.FindElements(By.CssSelector("a[data-marker='rubricator/row/link'] span"));*/
                                    ReadOnlyCollection<IWebElement> models = driver.FindElements(By.CssSelector(".g6gv8w4.g6gv8w8._501ok20"));
                                    
    
                                    List<CarModel> carModels = new List<CarModel>();
                                    
                                    foreach (var model in models)
                                    {
                                        string modelName = model.Text;
                                        currentWindowHandle = driver.CurrentWindowHandle;
                                        List<string> generationList = new List<string>();
                                        actions.MoveToElement(model)
                                            .KeyDown(Keys.Control)
                                            .Click(model)
                                            .KeyUp(Keys.Control)
                                            .Perform();
                                        Thread.Sleep(1000);

                                        var tabs_2 = driver.WindowHandles;
                                        driver.SwitchTo().Window(tabs_2.Last());

                                        ReadOnlyCollection<IWebElement> generation_all_1 = driver.FindElements(By.CssSelector("div[class='css-pyemnz e1ei9t6a4']"));

                                        foreach (var region in generation_all_1)
                                        {
                                            ReadOnlyCollection<IWebElement> generation_all =
                                                region.FindElements(
                                                    By.CssSelector(
                                                        "div[data-ga-stats-name='generations_outlet_item']"));
                                            string regi = region.Text;
                                            
                                            foreach (var generatio in generation_all)
                                            {
                                                IWebElement gen =
                                                    generatio.FindElement(By.CssSelector(".css-1089mxj.e1ei9t6a2"));
                                                IWebElement gen_2 = generatio.FindElement(
                                                    By.CssSelector(
                                                        "div[data-ftid='component_article_extended-info'] div"));

                                                string generation_1 = gen.Text;
                                                string generation_2 = gen_2.Text;
                                                generation_1 = generation_1.Replace("\r\n", "");
                                                generation_1 = generation_1.Replace("н.в.", null);
                                                generation_1 = generation_1.Replace(markName, "");
                                                generation_1 = generation_1.Replace(modelName, "");
                                                generation_1 = generation_1.Replace("  ", "");
                                                generation_1 = generation_1.Replace("-n", "- n");
                                                string generation = generation_1 + " " + generation_2;
                                                string pattern = @"\([^)]*\)";
                                                string result = Regex.Replace(generation, pattern, "");

                                                if (regi.Contains("России"))
                                                {
                                                    result = "Р " + result;
                                                }
                                                generationList.Add(result);

                                            }

                                        }

                                        carModels.Add(new CarModel
                                        {
                                            Model = modelName,
                                            Generations = generationList
                                        });
                                        driver.Close();
                                        driver.SwitchTo().Window(currentWindowHandle);

                                    }
                                    CarMake carMake = new CarMake
                                    {
                                        Make = markName,
                                        Models = carModels
                                    };
                                    driver.Close();
                                    driver.SwitchTo().Window(tabs.First());

                                    string jsonString = JsonConvert.SerializeObject(carMake, Formatting.Indented);

                                    File.WriteAllText("manufacturer_" + markName + ".json", jsonString, Encoding.UTF8);


                                    Console.WriteLine($"Данные успешно сохранены в manufacturer_{markName}.json");
                                }
                                catch (NoSuchElementException e)
                                {
                                    Console.WriteLine("Element not found: " + e.Message);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("Exception: " + e.Message);
                                }

                                count = 0;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                driver.Quit();
            }
        }
    }
}
