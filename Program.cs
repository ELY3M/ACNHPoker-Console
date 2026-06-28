// See https://aka.ms/new-console-template for more information
using ACNHPokerCore;
using Controller;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.RegularExpressions;

Console.WriteLine("ACNHPoker Console");


    bool keepRunning = true;
    Socket socket = null;
    //updated offset to work on latest verion of ACNH Game.  
    string chat = "[main+5255A60]+40"; //"[main+5254A40]+40";//"[main+4AA9CD8]+40";
    bool sendLock = false;

    String getipaddress = "";
    String chattext = "";
    bool connecting = false;
    bool chatting = false;

   int GameCap = 24;

   byte[] header;

//DataTable itemSource;
//DataTable recipeSource;
//DataTable flowerSource;
//DataTable variationSource;


controller controller = null;       

Console.WriteLine("Enter Switch IP Address:");

/*
 * you can save your switch's ip address in a file called ip.txt in the same directory as this program.
*/

if (File.Exists(@"ip.txt")) {
    string getipaddressfile = File.ReadAllText(@"ip.txt");
    getipaddress = getipaddressfile;
    Connect(getipaddress);
}
else
{
    getipaddress = Console.ReadLine();
    Connect(getipaddress);
}



while (keepRunning)
{
    Console.WriteLine("=== MAIN MENU ===");
    Console.WriteLine("1) Chat");
    Console.WriteLine("2) Read your pockets and list items you have in your pockets");
    Console.WriteLine("3) Fill your pockets (.nhi item)");
    Console.WriteLine("4) Freeze (auto refill)");
    Console.WriteLine("5) Unfreeze (stop refilling)");
    Console.WriteLine("6) Exit");
    Console.Write("\r\nSelect an option: ");

    string menuSelection = Console.ReadLine();
    
    switch (menuSelection)
    {
        case "1":
            Chatstart();
            break;
        case "2":
            Console.WriteLine("Reading your pockets");
            UpdateInventory();
            break;
        case "3":
            Console.WriteLine("Loading .nhinot implemented yet");
            break;
        case "4":
            Console.WriteLine("Freezing your inventory");
            invfreeze();
            break;
        case "5":
            Console.WriteLine("Unfreezing your inventory");
            invunfreeze();
            break;
        case "6":
            Console.WriteLine("\nExiting...");
            keepRunning = false;
            break;
        case "e":
            Console.WriteLine("\nExiting...");
            keepRunning = false;
            break;
        case "exit":
            Console.WriteLine("\nExiting...");
            keepRunning = false;
            break;
        default:
            Console.WriteLine("\nInvalid option. Press any key to try again.");
            break;
    }
    
    if (keepRunning && menuSelection != "6")
    {
        Console.WriteLine("\nPress any key to return to the menu...");
        Console.ReadKey();
    }


}



void UpdateInventory()
{
    //AllowInventoryUpdate = false;
    Console.WriteLine("Reading Inventory...");

    DataTable itemSource = LoadItemCSV(Utilities.itemPath);
    DataTable recipeSource = LoadItemCSV(Utilities.recipePath);
    DataTable flowerSource = LoadItemCSV(Utilities.flowerPath);
    DataTable variationSource = LoadItemCSV(Utilities.variationPath);

try
    {
        byte[] bank01To20 = Utilities.GetInventoryBank(socket, 1);
        if (bank01To20 == null)
        {
            return;
        }
        byte[] bank21To40 = Utilities.GetInventoryBank(socket, 21);
        if (bank21To40 == null)
        {
            return;
        }

        string Bank1 = Utilities.ByteToHexString(bank01To20);
        string Bank2 = Utilities.ByteToHexString(bank21To40);

        Console.WriteLine("\nBank1:\n");
        Console.WriteLine(Bank1);
        Console.WriteLine("\n====================================\nBank2:\n");
        Console.WriteLine(Bank2);
        Console.WriteLine("\n");


        //int slotId = int.Parse(Tag.ToString());

        for (int slotId = 1; slotId <= 40; slotId++)
        {

            byte[] slotBytes = new byte[2];
            byte[] flag0Bytes = new byte[1];
            byte[] flag1Bytes = new byte[1];
            byte[] dataBytes = new byte[4];
            byte[] recipeBytes = new byte[2];
            byte[] fenceBytes = new byte[2];

            int slotOffset;
            int countOffset;
            int flag0Offset;
            int flag1Offset;
            if (slotId < 21)
            {
                slotOffset = ((slotId - 1) * 0x8);
                flag0Offset = 0x3 + ((slotId - 1) * 0x8);
                flag1Offset = 0x2 + ((slotId - 1) * 0x8);
                countOffset = 0x4 + ((slotId - 1) * 0x8);
            }
            else
            {
                slotOffset = ((slotId - 21) * 0x8);
                flag0Offset = 0x3 + ((slotId - 21) * 0x8);
                flag1Offset = 0x2 + ((slotId - 21) * 0x8);
                countOffset = 0x4 + ((slotId - 21) * 0x8);
            }

            if (slotId < 21)
            {
                Buffer.BlockCopy(bank01To20, slotOffset, slotBytes, 0x0, 0x2);
                Buffer.BlockCopy(bank01To20, flag0Offset, flag0Bytes, 0x0, 0x1);
                Buffer.BlockCopy(bank01To20, flag1Offset, flag1Bytes, 0x0, 0x1);
                Buffer.BlockCopy(bank01To20, countOffset, dataBytes, 0x0, 0x4);
                Buffer.BlockCopy(bank01To20, countOffset, recipeBytes, 0x0, 0x2);
                Buffer.BlockCopy(bank01To20, countOffset + 0x2, fenceBytes, 0x0, 0x2);
            }
            else
            {
                Buffer.BlockCopy(bank21To40, slotOffset, slotBytes, 0x0, 0x2);
                Buffer.BlockCopy(bank21To40, flag0Offset, flag0Bytes, 0x0, 0x1);
                Buffer.BlockCopy(bank21To40, flag1Offset, flag1Bytes, 0x0, 0x1);
                Buffer.BlockCopy(bank21To40, countOffset, dataBytes, 0x0, 0x4);
                Buffer.BlockCopy(bank21To40, countOffset, recipeBytes, 0x0, 0x2);
                Buffer.BlockCopy(bank21To40, countOffset + 0x2, fenceBytes, 0x0, 0x2);
            }

            string itemId = Utilities.Flip(Utilities.ByteToHexString(slotBytes));
            string itemData = Utilities.Flip(Utilities.ByteToHexString(dataBytes));
            string recipeData = Utilities.Flip(Utilities.ByteToHexString(recipeBytes));
            string fenceData = Utilities.Flip(Utilities.ByteToHexString(fenceBytes));
            string flag0 = Utilities.ByteToHexString(flag0Bytes);
            string flag1 = Utilities.ByteToHexString(flag1Bytes);
            UInt16 intId = Convert.ToUInt16(itemId, 16);

            string itemName = Utilities.GetNameFromID(itemId, itemSource);

            Console.WriteLine("Slot : " + slotId.ToString() + " ID : " + itemId + " Count : "+ (Convert.ToUInt32("0x" + itemData, 16) + 1) +" Data : " + itemData + " recipeData : " + recipeData + " Flag0 : " + flag0 + " Flag1 : " + flag1);

            //Console.WriteLine("Item Name : " + itemName);

            //Convert.ToUInt16("0x" + itemId, 16)
            //Console.WriteLine("Count: " + (Convert.ToUInt32("0x" + itemData, 16) + 1));


            if (itemId == "FFFE") //Nothing
            {
                Console.WriteLine("Slot : " + slotId.ToString() + " is empty.");
            }
            else if (itemId == "16A2") //Recipe
            {
                Console.WriteLine("Slot: " + slotId.ToString() + " is a recipe for " + Utilities.GetNameFromID(recipeData, recipeSource));
            }
            else if (itemId == "1095") //Delivery
            {
                Console.WriteLine("Slot: " + slotId.ToString() + " is a delivery for " + Utilities.GetNameFromID(recipeData, itemSource));
            }
            else if (itemId == "16A1") //Bottle Message
            {
               //btn.Setup(Utilities.GetNameFromID(recipeData, recipeSource), 0x16A1, Convert.ToUInt32("0x" + itemData, 16), GetImagePathFromID(recipeData, recipeSource), "", flag0, flag1);
               Console.WriteLine("Slot: " + slotId.ToString() + " is a bottle message for " + Utilities.GetNameFromID(recipeData, recipeSource));
            }
            else if (itemId == "0A13") // Fossil
            {
                //btn.Setup(Utilities.GetNameFromID(recipeData, itemSource), 0x0A13, Convert.ToUInt32("0x" + itemData, 16), GetImagePathFromID(recipeData, itemSource), "", flag0, flag1);
                Console.WriteLine("Slot: " + slotId.ToString() + " is a fossil for " + Utilities.GetNameFromID(recipeData, itemSource));
            }
            else if (itemId == "114A") // Money Tree
            {
                //btn.Setup(Utilities.GetNameFromID(itemId, itemSource), Convert.ToUInt16("0x" + itemId, 16), Convert.ToUInt32("0x" + itemData, 16), GetImagePathFromID(itemId, itemSource, Convert.ToUInt32("0x" + itemData, 16)), GetImagePathFromID(recipeData, itemSource), flag0, flag1);
                Console.WriteLine("Slot: " + slotId.ToString() + " is a money tree for " + Utilities.GetNameFromID(itemId, itemSource));
            }
            else if (itemId == "315A" || itemId == "1618" || itemId == "342F") // Wall-Mounted
            {
                //btn.Setup(Utilities.GetNameFromID(itemId, itemSource), Convert.ToUInt16("0x" + itemId, 16), Convert.ToUInt32("0x" + itemData, 16), GetImagePathFromID(itemId, itemSource, Convert.ToUInt32("0x" + itemData, 16)), GetImagePathFromID(recipeData, itemSource, Convert.ToUInt32("0x" + Utilities.TranslateVariationValueBack(fenceData), 16)), flag0, flag1);
                Console.WriteLine("Slot: " + slotId.ToString() + " is a wall-mounted item for " + Utilities.GetNameFromID(itemId, itemSource));
            }
            else if (ItemAttr.HasFenceWithVariation(intId)) // Fence Variation
            {
                //btn.Setup(Utilities.GetNameFromID(itemId, itemSource), Convert.ToUInt16("0x" + itemId, 16), Convert.ToUInt32("0x" + itemData, 16), GetImagePathFromID(itemId, itemSource, Convert.ToUInt32("0x" + fenceData, 16)), "", flag0, flag1);
                Console.WriteLine("Slot: " + slotId.ToString() + " is a fence variation for " + Utilities.GetNameFromID(itemId, itemSource));
            }
            else
            {
                //btn.Setup(GetNameFromID(itemId, itemSource), Convert.ToUInt16("0x" + itemId, 16), Convert.ToUInt32("0x" + itemData, 16), GetImagePathFromID(itemId, itemSource, Convert.ToUInt32("0x" + itemData, 16)), "", flag0, flag1);
                Console.WriteLine("Slot: " + slotId.ToString() + " is an item for " + Utilities.GetNameFromID(itemId, itemSource));

            }
            
        }
    }

    catch (Exception ex)
    {
        //MyLog.LogEvent("MainForm", "UpdateInventory: " + ex.Message);
        //Invoke((MethodInvoker)delegate { InventoryAutoRefreshToggle.Checked = false; });
        //InventoryRefreshTimer.Stop();
        Console.WriteLine(ex.Message + " This seems like a bad idea but it's fine for now.");
        return;
    }

    //AllowInventoryUpdate = true;
}

string[] GetInventoryName()
{
    string[] namelist = new string[8];
    //Debug.Print("Peek 8 Name:");
    byte[] tempHeader = null;
    Boolean headerFound = false;

    for (int i = 0; i < 8; i++)
    {
        byte[] b = Utilities.GetInventoryName(socket, i);
        if (b == null)
        {
            namelist[i] = "NULL";
        }
        else
            namelist[i] = Encoding.Unicode.GetString(b, 32, 20);
        namelist[i] = namelist[i].Replace("\0", string.Empty);
        if (namelist[i].Equals(string.Empty) && !headerFound)
        {
            header = tempHeader;
            headerFound = true;
        }
        tempHeader = b;
    }
    return namelist;
}

byte[] GetHeader()
{
    return header;
}


static DataTable LoadItemCSV(string filePath)
{
    var dt = new DataTable();

    File.ReadLines(filePath).Take(1)
        .SelectMany(x => x.Split([" ; "], StringSplitOptions.RemoveEmptyEntries))
        .ToList()
        .ForEach(x => dt.Columns.Add(x.Trim()));

    File.ReadLines(filePath).Skip(1)
        .Select(x => x.Split([" ; "], StringSplitOptions.RemoveEmptyEntries))
        .ToList()
        .ForEach(line => dt.Rows.Add(line));

    if (dt.Columns.Contains("id"))
        dt.PrimaryKey = [dt.Columns["id"]];

    return dt;
}

static DataTable LoadCSVwoKey(string filePath)
{
    var dt = new DataTable();

    File.ReadLines(filePath).Take(1)
        .SelectMany(x => x.Split([" ; "], StringSplitOptions.RemoveEmptyEntries))
        .ToList()
        .ForEach(x => dt.Columns.Add(x.Trim()));

    File.ReadLines(filePath).Skip(1)
        .Select(x => x.Split([" ; "], StringSplitOptions.RemoveEmptyEntries))
        .ToList()
        .ForEach(line => dt.Rows.Add(line));

    return dt;
}

static byte[] LoadBinaryFile(string file)
{
    return File.Exists(file) ? File.ReadAllBytes(file) : null;
}
static Dictionary<string, string> CreateOverride(string path)
{
    Dictionary<string, string> dict = [];

    if (!File.Exists(path)) return dict;

    string[] lines = File.ReadAllLines(path);

    foreach (string line in lines)
    {
        string[] parts = line.Split([" ; "], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 3)
        {
            dict.Add(parts[1], parts[2]);
        }
    }

    return dict;
}


/*
public string GetNameFromID(string itemID, DataTable source)
{
    if (source == null)
    {
        return "";
    }

    DataRow row = source.Rows.Find(itemID);

    if (row == null)
    {
        return ""; //row not found
    }
    else
    {
        //row found set the index and find the name
        return (string)row["eng"];
    }
}

public string GetNameFromIDr(string itemID, bool IsRecipe = false)
{
    if (!IsRecipe)
        return GetNameFromID(itemID, itemSource);
    else
        return GetNameFromID(itemID, recipeSource);
}
*/


void invfreeze() 
{

    byte[] bank01To20 = Utilities.GetInventoryBank(socket, 1);
    byte[] bank21To40 = Utilities.GetInventoryBank(socket, 21);
    Utilities.SendString(socket, Utilities.Freeze(Utilities.ItemSlotBase, bank01To20));
    Utilities.SendString(socket, Utilities.Freeze(Utilities.ItemSlot21Base, bank21To40));

}

void invunfreeze()
{
    Utilities.SendString(socket, Utilities.UnFreeze(Utilities.ItemSlotBase));
    Utilities.SendString(socket, Utilities.UnFreeze(Utilities.ItemSlot21Base));
}


void Chatstart()
{
    Startchat();
    while (controller != null)
    {
        Console.WriteLine("Enter Chat Message:");
        chattext = Console.ReadLine();
        if (chattext.Length <= GameCap)
        {
            sendchat(socket, chattext);
        }
        else
        {
            Console.WriteLine("Chat Text exceeds " + GameCap + " character limit.");
        }

    }
}


void Connect(String getipaddress)
    {

        if (getipaddress == null)
        {
            Console.WriteLine("You must enter your switch ip address");
            return;
        }

        ///Preferences.Default.Set("ipaddress", getipaddress);


        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint ep = new IPEndPoint(IPAddress.Parse(getipaddress), 6000);

        IAsyncResult result = socket.BeginConnect(ep, null, null);
        bool conSuceded = result.AsyncWaitHandle.WaitOne(3000, true);
    if (conSuceded == true)
    {

        Console.WriteLine("Connected to "+getipaddress);
        controller = new controller(socket);
    }
    else
    {
        Console.WriteLine("Failed to connect to "+getipaddress);
        socket.Close();
        socket = null;
    }


    }


    void Startchat()
    {
        controller.detachController();
        controller.clickA();
        controller.clickZR();
        Thread.Sleep(1000);
        controller.clickB();
        controller.clickB();
        controller.clickB();
    }

    void Sendchat(Socket socket, String chattext)
    {

        Chat(socket);
        string cleanStr = chattext.Trim().Replace("\n", " ");

        if (sendLock)
            return;
        if (cleanStr.Equals(""))
            return;


        sendLock = true;

        Thread sendThread = new Thread(delegate () { sendChat(socket, cleanStr); });
        sendThread.Start();


    }

    static void SendString(Socket socket, byte[] buffer, int offset = 0, int size = 0, int timeout = 100)
    {
        int startTickCount = Environment.TickCount;
        int sent = 0;  // how many bytes is already sent
        if (size == 0)
            for (int i = offset; i < buffer.Length; i++)
                if (buffer[i] == 0xA)
                {
                    size = i + 1 - offset;
                    break;
                }
        if (size == 0) size = buffer.Length - offset;
        do
        {
            if (Environment.TickCount > startTickCount + timeout)
                throw new Exception("Timeout.");
            try
            {
                sent += socket.Send(buffer, offset + sent, size - sent, SocketFlags.None);
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.WouldBlock ||
                    ex.SocketErrorCode == SocketError.IOPending ||
                    ex.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                {
                    // socket buffer is probably full, wait and try again
                    //Thread.Sleep(10);
                }
                else
                    throw;  // any serious error occurr
            }
        } while (sent < size);
    }
    static int ReceiveString(Socket socket, byte[] buffer, int offset = 0, int size = 0, int timeout = 30000)
    {
        int startTickCount = Environment.TickCount;
        int received = 0;  // how many bytes is already received
        if (size == 0) size = buffer.Length - offset;
        do
        {
            if (Environment.TickCount > startTickCount + timeout)
                throw new Exception("Timeout.");
            try
            {
                received += socket.Receive(buffer, offset + received, size - received, SocketFlags.None);
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.WouldBlock ||
                    ex.SocketErrorCode == SocketError.IOPending ||
                    ex.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                {
                    // socket buffer is probably empty, wait and try again
                    //Thread.Sleep(30);
                }
                else
                    throw;  // any serious error occurr
            }
        } while (received < size && buffer[received - 1] != 0xA);
        return received;
    }


    static void pokeAbsoluteAddress(Socket socket, string address, string value)
    {
        //lock (botLock)
        {
            string msg = String.Format("pokeAbsolute 0x{0:X8} 0x{1}\r\n", address, value);
            SendString(socket, Encoding.UTF8.GetBytes(msg));
        }
    }


    static string ByteToHexString(byte[] b)
    {
        String hexString = BitConverter.ToString(b);
        hexString = hexString.Replace("-", "");

        return hexString;
    }

    static byte[] ByteTrim(byte[] input)
    {
        int newLength = 1;
        for (int i = 0; i < input.Length; i++)
        {
            if (input[i] == 0x0)
            {
                newLength = i;
                break;
            }
        }

        byte[] newArray = new byte[newLength];
        Array.Copy(input, newArray, newArray.Length);

        return newArray;
    }

    static byte[] peekMainAddress(Socket socket, string address, int size)
    {
        //lock (botLock)
        {
            byte[] result = new byte[size];

            string msg = String.Format("peekMain 0x{0:X8} 0x{1}\r\n", address, size);
            //Debug.Print("PeekMain : " + msg);
            SendString(socket, Encoding.UTF8.GetBytes(msg));

            byte[] b = new byte[size * 2 + 64];
            int first_rec = ReceiveString(socket, b);
            string buffer = Encoding.ASCII.GetString(b, 0, size * 2);

            if (buffer == null)
            {
                return null;
            }
            for (int i = 0; i < size; i++)
            {
                result[i] = Convert.ToByte(buffer.Substring(i * 2, 2), 16);
            }

            return result;
        }
    }

    static byte[] peekAbsoluteAddress(Socket socket, string address, int size)
    {
        //lock (botLock)
        {
            byte[] result = new byte[size];

            string msg = String.Format("peekAbsolute 0x{0:X8} {1}\r\n", address, size);
            SendString(socket, Encoding.UTF8.GetBytes(msg));
            byte[] b = new byte[size * 2 + 64];
            int first_rec = ReceiveString(socket, b);
            string buffer = Encoding.ASCII.GetString(b, 0, size * 2);

            if (buffer == null)
            {
                return null;
            }
            for (int i = 0; i < size; i++)
            {
                result[i] = Convert.ToByte(buffer.Substring(i * 2, 2), 16);
            }

            return result;
        }
    }

    /*
    public static void pokeAbsoluteAddress(Socket socket, string address, string value)
    {
        //lock (botLock)
        {
            string msg = String.Format("pokeAbsolute 0x{0:X8} 0x{1}\r\n", address, value);
            SendString(socket, Encoding.UTF8.GetBytes(msg));
        }
    }
    */
    static ulong GetCoordinateAddress(string strInput, Socket s)
    {
        //lock (lockObject)
        {
            // Regex pattern to get operators and offsets from pointer expression.	
            string pattern = @"(\+|\-)([A-Fa-f0-9]+)";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(strInput);

            // Get first offset from pointer expression and read address at that offset from main start.	
            var ofs = Convert.ToUInt64(match.Groups[2].Value, 16);
            var address = BitConverter.ToUInt64(peekMainAddress(s, ofs.ToString("X"), 0x8), 0);
            match = match.NextMatch();

            // Matches the rest of the operators and offsets in the pointer expression.	
            while (match.Success)
            {
                // Get operator and offset from match.	
                string opp = match.Groups[1].Value;
                ofs = Convert.ToUInt64(match.Groups[2].Value, 16);

                // Add or subtract the offset from the current stored address based on operator in front of offset.	
                switch (opp)
                {
                    case "+":
                        address += ofs;
                        break;
                    case "-":
                        address -= ofs;
                        break;
                }

                // Attempt another match and if successful read bytes at address and store the new address.	
                match = match.NextMatch();
                if (match.Success)
                {
                    byte[] bytes = peekAbsoluteAddress(s, address.ToString("X"), 0x8);
                    address = BitConverter.ToUInt64(bytes, 0);
                }
            }

            return address;
        }
    }



    void Chat(Socket Socket)
    {
        socket = Socket;

        ///InitializeComponent();            
        ///chattext.SelectAll();
        //chattext.CursorPosition = 0;
        //chattext.SelectionLength = chattext?.Length ?? 0;

        

    }


    void sendchat(Socket socket, String chattext)
    {
        string cleanStr = chattext.Trim().Replace("\n", " ");

        if (sendLock)
            return;
        if (cleanStr.Equals(""))
            return;


        sendLock = true;

        Thread sendThread = new Thread(delegate () { sendChat(socket, cleanStr); });
        sendThread.Start();
    }

    void sendChat(Socket socket, string message)
    {
        ulong ChatAddress = GetCoordinateAddress(chat, socket);

        controller.clickR();
        Thread.Sleep(800);
        controller.clickY();

        byte[] StrBytes = Encoding.Unicode.GetBytes(message);
        byte[] sendBytes = new byte[StrBytes.Length * 2];
        Buffer.BlockCopy(StrBytes, 0, sendBytes, 0, StrBytes.Length);
        pokeAbsoluteAddress(socket, ChatAddress.ToString("X"), ByteToHexString(sendBytes));

        controller.clickPLUS();
        Thread.Sleep(400);

        controller.clickB();
        Thread.Sleep(200);
        controller.clickB();
        Thread.Sleep(200);
        controller.clickB();
        Thread.Sleep(200);

        sendLock = false;
    }
