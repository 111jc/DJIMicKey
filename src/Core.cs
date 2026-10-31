using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using Timer = System.Windows.Forms.Timer;

namespace DjiMicKey {
    static class Native {

        [DllImport("dwmapi.dll")] public static extern int DwmSetWindowAttribute(IntPtr window,int attribute,ref int value,int size);

        [StructLayout(LayoutKind.Sequential,Pack=1)] public struct Blend {public byte Op,Flags,Alpha,Format;}
        [DllImport("user32.dll")] public static extern IntPtr GetDC(IntPtr window);
        [DllImport("user32.dll")] public static extern int ReleaseDC(IntPtr window,IntPtr dc);
        [DllImport("gdi32.dll")] public static extern IntPtr CreateCompatibleDC(IntPtr dc);
        [DllImport("gdi32.dll")] public static extern bool DeleteDC(IntPtr dc);
        [DllImport("gdi32.dll")] public static extern IntPtr SelectObject(IntPtr dc,IntPtr obj);
        [DllImport("gdi32.dll")] public static extern bool DeleteObject(IntPtr obj);
        [DllImport("user32.dll",SetLastError=true)] public static extern bool UpdateLayeredWindow(IntPtr window,IntPtr screen,ref Point target,ref Size size,IntPtr source,ref Point origin,int key,ref Blend blend,int flags);
        [DllImport("user32.dll",EntryPoint="GetWindowLongPtrW")] public static extern IntPtr GetWindowStyle(IntPtr window,int index);
        public static void DisableNativeFrame(IntPtr window){try{int policy=1;DwmSetWindowAttribute(window,2,ref policy,4);}catch(DllNotFoundException){}catch(EntryPointNotFoundException){}}
        public const int WM_DEVICECHANGE = 0x219;
        [StructLayout(LayoutKind.Sequential)] public struct Rect {public int Left,Top,Right,Bottom;}
        [DllImport("user32.dll")] public static extern IntPtr SetThreadDpiAwarenessContext(IntPtr context);
        [DllImport("user32.dll")] public static extern uint GetDpiForWindow(IntPtr window);
        public static void EnableMonitorDpi(){try{SetThreadDpiAwarenessContext(new IntPtr(-4));}catch(EntryPointNotFoundException){}catch(DllNotFoundException){}}
        public static int WindowDpi(IntPtr window){try{uint dpi=GetDpiForWindow(window);if(dpi>0)return (int)dpi;}catch(EntryPointNotFoundException){}using(Graphics graphics=Graphics.FromHwnd(window))return (int)graphics.DpiX;}
        public const uint Injected = 0x10;
        public static readonly UIntPtr Marker = new UIntPtr(0x444A494D);
        public delegate IntPtr HookProc(int code, IntPtr message, IntPtr data);
        [StructLayout(LayoutKind.Sequential)] public struct Kbd { public uint vk, scan, flags, time; public UIntPtr extra; }
        [StructLayout(LayoutKind.Sequential)] public struct RawDevice { public IntPtr handle; public uint type; }
        [StructLayout(LayoutKind.Sequential)] public struct KeyboardInput { public ushort vk, scan; public uint flags, time; public UIntPtr extra; }
        [StructLayout(LayoutKind.Sequential)] public struct MouseInput { public int x, y; public uint data, flags, time; public UIntPtr extra; }
        [StructLayout(LayoutKind.Explicit)] public struct InputUnion { [FieldOffset(0)] public MouseInput mouse; [FieldOffset(0)] public KeyboardInput keyboard; }
        [StructLayout(LayoutKind.Sequential)] public struct Input { public uint type; public InputUnion data; }
        [DllImport("user32.dll", SetLastError=true)] public static extern IntPtr SetWindowsHookEx(int id, HookProc proc, IntPtr module, uint thread);
        [DllImport("user32.dll")] public static extern bool UnhookWindowsHookEx(IntPtr hook);
        [DllImport("user32.dll")] public static extern IntPtr CallNextHookEx(IntPtr hook, int code, IntPtr message, IntPtr data);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode)] public static extern IntPtr GetModuleHandle(string module);
        [DllImport("user32.dll", SetLastError=true)] public static extern uint SendInput(uint count, Input[] inputs, int size);
        [DllImport("user32.dll")] public static extern uint MapVirtualKey(uint code, uint type);
        [DllImport("user32.dll")] public static extern short GetAsyncKeyState(int key);
        [DllImport("user32.dll", SetLastError=true)] public static extern uint GetRawInputData(IntPtr raw,uint command,IntPtr data,ref uint size,uint header);
        [DllImport("user32.dll", SetLastError=true)] public static extern uint GetRawInputDeviceList(IntPtr list, ref uint count, uint size);
        [DllImport("user32.dll", CharSet=CharSet.Unicode, SetLastError=true)] public static extern uint GetRawInputDeviceInfo(IntPtr device, uint command, StringBuilder data, ref uint size);
        [DllImport("user32.dll", CharSet=CharSet.Unicode)] public static extern uint RegisterWindowMessage(string text);
        [DllImport("user32.dll")] public static extern bool PostMessage(IntPtr hwnd, uint message, IntPtr wparam, IntPtr lparam);
        [StructLayout(LayoutKind.Sequential)] public struct Message {public IntPtr hwnd;public uint message;public UIntPtr wparam;public IntPtr lparam;public uint time;public int x,y;public uint privateData;}
        [DllImport("user32.dll")] public static extern bool PeekMessage(out Message message,IntPtr window,uint min,uint max,uint remove);
        [DllImport("user32.dll")] public static extern int GetMessage(out Message message,IntPtr window,uint min,uint max);
        [DllImport("user32.dll")] public static extern bool TranslateMessage(ref Message message);
        [DllImport("user32.dll")] public static extern IntPtr DispatchMessage(ref Message message);
        [DllImport("user32.dll")] public static extern bool PostThreadMessage(uint thread,uint message,UIntPtr wparam,IntPtr lparam);
        [DllImport("kernel32.dll")] public static extern uint GetCurrentThreadId();
        [DllImport("user32.dll")] public static extern bool DestroyIcon(IntPtr icon);
        [DllImport("user32.dll")] public static extern bool ReleaseCapture();
        [DllImport("user32.dll")] public static extern bool GetCursorPos(out Point point);
        [DllImport("user32.dll")] public static extern IntPtr SendMessage(IntPtr hwnd,uint message,IntPtr wparam,IntPtr lparam);
        [DllImport("ole32.dll")] public static extern int PropVariantClear(ref PropVariant value);
        public static void Release(object value) { if (value != null && Marshal.IsComObject(value)) Marshal.ReleaseComObject(value); }
        public static string Error(Exception ex) { return ex.Message + " (0x" + ex.HResult.ToString("X8") + ")"; }
    }

    struct KeyDefinition {
        public string Id, Label; public ushort Vk;
        public KeyDefinition(string id, string label, int vk) { Id=id; Label=label; Vk=(ushort)vk; }
        public override string ToString() { return Label; }
    }
    sealed class ShortcutSpec {
        public bool Ctrl=true, Alt=true, Shift=false, Win=false;
        public string Key="F9";
        public List<ushort> RecordedKeys;
        public static readonly List<KeyDefinition> Keys = CreateKeys();
        static List<KeyDefinition> CreateKeys() {
            List<KeyDefinition> list=new List<KeyDefinition>();
            for(int i=1;i<=24;i++) list.Add(new KeyDefinition("F"+i,"F"+i,0x6F+i));
            for(int i=65;i<=90;i++) list.Add(new KeyDefinition(((char)i).ToString(),((char)i).ToString(),i));
            for(int i=48;i<=57;i++) list.Add(new KeyDefinition(((char)i).ToString(),((char)i).ToString(),i));
            list.AddRange(new KeyDefinition[] {
                new KeyDefinition("Space","空格",0x20),new KeyDefinition("Enter","Enter",0x0D),
                new KeyDefinition("Tab","Tab",0x09),new KeyDefinition("Escape","Esc",0x1B),
                new KeyDefinition("Backspace","Backspace",0x08),new KeyDefinition("Delete","Delete",0x2E),
                new KeyDefinition("Insert","Insert",0x2D),new KeyDefinition("Home","Home",0x24),new KeyDefinition("End","End",0x23),
                new KeyDefinition("PageUp","Page Up",0x21),new KeyDefinition("PageDown","Page Down",0x22),
                new KeyDefinition("Up","↑",0x26),new KeyDefinition("Down","↓",0x28),new KeyDefinition("Left","←",0x25),new KeyDefinition("Right","→",0x27),
                new KeyDefinition("CapsLock","Caps Lock",0x14),new KeyDefinition("PrintScreen","Print Screen",0x2C),new KeyDefinition("Pause","Pause",0x13),
                new KeyDefinition("LCtrl","左 Ctrl",0xA2),new KeyDefinition("RCtrl","右 Ctrl",0xA3),
                new KeyDefinition("LAlt","左 Alt",0xA4),new KeyDefinition("RAlt","右 Alt",0xA5),
                new KeyDefinition("LShift","左 Shift",0xA0),new KeyDefinition("RShift","右 Shift",0xA1),
                new KeyDefinition("LWin","左 Win",0x5B),new KeyDefinition("RWin","右 Win",0x5C)
            });
            for(int i=0;i<=9;i++) list.Add(new KeyDefinition("Num"+i,"小键盘 "+i,0x60+i));
            list.AddRange(new KeyDefinition[]{new KeyDefinition("Semicolon",";",0xBA),new KeyDefinition("Equals","=",0xBB),new KeyDefinition("Comma",",",0xBC),new KeyDefinition("Minus","-",0xBD),new KeyDefinition("Period",".",0xBE),new KeyDefinition("Slash","/",0xBF),new KeyDefinition("Backtick","\u0060",0xC0),new KeyDefinition("LeftBracket","[",0xDB),new KeyDefinition("Backslash","\\",0xDC),new KeyDefinition("RightBracket","]",0xDD),new KeyDefinition("Quote","'",0xDE),new KeyDefinition("NumAdd","小键盘 +",0x6B),new KeyDefinition("NumSubtract","小键盘 -",0x6D),new KeyDefinition("NumMultiply","小键盘 *",0x6A),new KeyDefinition("NumDivide","小键盘 /",0x6F),new KeyDefinition("NumDecimal","小键盘 .",0x6E),new KeyDefinition("MediaPlayPause","播放 / 暂停",0xB3)});
            return list;
        }
        public static bool IsModifier(ushort key){return key==0x5B||key==0x5C||(key>=0xA0&&key<=0xA5);}
        public static KeyDefinition Find(ushort key){foreach(KeyDefinition k in Keys)if(k.Vk==key)return k;throw new ArgumentException("暂不支持这个按键，请重新录制。");}
        public static ushort Normalize(uint vk,uint scan,uint flags){if(vk==0x11)return (ushort)((flags&1)!=0?0xA3:0xA2);if(vk==0x12)return (ushort)((flags&1)!=0?0xA5:0xA4);if(vk==0x10)return (ushort)(scan==0x36?0xA1:0xA0);return (ushort)vk;}
        public static ShortcutSpec FromKeys(IEnumerable<ushort> values){List<ushort> modifiers=new List<ushort>(),main=new List<ushort>();foreach(ushort k in values){if(IsModifier(k))modifiers.Add(k);else main.Add(k);}modifiers.AddRange(main);ShortcutSpec result=new ShortcutSpec {Ctrl=false,Alt=false,Shift=false,Win=false,RecordedKeys=modifiers};result.Validate();return result;}
        public KeyDefinition Definition { get { foreach(KeyDefinition k in Keys) if(k.Id==Key) return k; throw new ArgumentException("不支持的目标按键"); } }
        public void Validate() {
            if(RecordedKeys!=null){if(RecordedKeys.Count==0||RecordedKeys.Count>9)throw new ArgumentException("请录制一个按键或组合键。");HashSet<ushort> unique=new HashSet<ushort>();int mainCount=0;foreach(ushort k in RecordedKeys){Find(k);if(!unique.Add(k))throw new ArgumentException("快捷键包含重复按键。");if(!IsModifier(k))mainCount++;}if(mainCount>1)throw new ArgumentException("组合键只支持修饰键加一个普通按键，请重新录制。");return;}
            ushort vk=Definition.Vk;
            if ((Ctrl&&(vk==0xA2||vk==0xA3)) || (Alt&&(vk==0xA4||vk==0xA5)) || (Shift&&(vk==0xA0||vk==0xA1)) || (Win&&(vk==0x5B||vk==0x5C)))
                throw new ArgumentException("主按键不能与勾选的修饰键重复。");
        }
        public ShortcutSpec Copy() { return new ShortcutSpec {Ctrl=Ctrl,Alt=Alt,Shift=Shift,Win=Win,Key=Key,RecordedKeys=RecordedKeys==null?null:new List<ushort>(RecordedKeys)}; }
        public string Display {
            get { if(RecordedKeys!=null){List<string> recorded=new List<string>();foreach(ushort key in RecordedKeys)recorded.Add(Find(key).Label);return String.Join(" + ",recorded.ToArray());}List<string> parts=new List<string>(); if(Ctrl)parts.Add("左 Ctrl");if(Alt)parts.Add("左 Alt");if(Shift)parts.Add("左 Shift");if(Win)parts.Add("左 Win");parts.Add(Definition.Label);return String.Join(" + ",parts.ToArray()); }
        }
        public List<ushort> VirtualKeys() {
            Validate();if(RecordedKeys!=null)return new List<ushort>(RecordedKeys); List<ushort> result=new List<ushort>();
            if(Ctrl)result.Add(0xA2);if(Alt)result.Add(0xA4);if(Shift)result.Add(0xA0);if(Win)result.Add(0x5B);result.Add(Definition.Vk);return result;
        }
        public Native.Input[] BuildInput() {
            List<ushort> keys=VirtualKeys(); List<Native.Input> inputs=new List<Native.Input>();
            foreach(ushort key in keys) inputs.Add(MakeInput(key,false));
            for(int i=keys.Count-1;i>=0;i--) inputs.Add(MakeInput(keys[i],true));
            return inputs.ToArray();
        }
        static Native.Input MakeInput(ushort vk, bool release) {
            uint scan=Native.MapVirtualKey(vk,4);
            Native.Input input=new Native.Input();input.type=1;
            input.data.keyboard=new Native.KeyboardInput {vk=vk,scan=(ushort)(scan&255),flags=(release?2U:0U)|((scan&0xFF00)!=0?1U:0U),extra=Native.Marker};
            return input;
        }
        public void Send() {
            // Preserve physically held keys: postpone instead of releasing somebody's Ctrl/Alt.
            Native.Input[] inputs=BuildInput();
            uint sent=Native.SendInput((uint)inputs.Length,inputs,Marshal.SizeOf(typeof(Native.Input)));
            if(sent!=inputs.Length) {
                // Release only keys this batch may have pressed, in reverse order.
                List<Native.Input> cleanup=new List<Native.Input>();
                for(int i=(int)Math.Min(sent,(uint)(inputs.Length/2))-1;i>=0;i--) { Native.Input up=inputs[i];up.data.keyboard.flags|=2;cleanup.Add(up); }
                if(cleanup.Count>0)Native.SendInput((uint)cleanup.Count,cleanup.ToArray(),Marshal.SizeOf(typeof(Native.Input)));
                throw new Win32Exception(Marshal.GetLastWin32Error(),"快捷键发送未完成，请确认目标窗口未以管理员权限运行。");
            }
        }
        public static bool OtherKeysHeld() {
            int[] keys={0xA0,0xA1,0xA2,0xA3,0xA4,0xA5,0x5B,0x5C};
            foreach(int k in keys) if((Native.GetAsyncKeyState(k)&0x8000)!=0)return true;return false;
        }
    }
    sealed class Settings {
        public ShortcutSpec Shortcut=new ShortcutSpec(); public bool Enabled=true; public bool CompatibilityMode=true;
        public static string DirectoryPath { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"DJIMicKey"); } }
        public static string FilePath { get { return Path.Combine(DirectoryPath,"settings.ini"); } }
        public static Settings Load(string path) {
            Settings s=new Settings(); if(!File.Exists(path))return s;
            if(new FileInfo(path).Length>4096)throw new FormatException("配置文件过大。");
            foreach(string line in File.ReadAllLines(path,Encoding.UTF8)) {
                int split=line.IndexOf('=');if(split<1)continue; string k=line.Substring(0,split).Trim(),v=line.Substring(split+1).Trim();
                switch(k) {case "CompatibilityMode":s.CompatibilityMode=ReadBool(v);break;case "Chord":List<ushort> chord=new List<ushort>();foreach(string token in v.Split(','))chord.Add(UInt16.Parse(token,NumberStyles.HexNumber,CultureInfo.InvariantCulture));s.Shortcut.RecordedKeys=chord;break;case "Key":s.Shortcut.Key=v;break;case "Ctrl":s.Shortcut.Ctrl=ReadBool(v);break;case "Alt":s.Shortcut.Alt=ReadBool(v);break;case "Shift":s.Shortcut.Shift=ReadBool(v);break;case "Win":s.Shortcut.Win=ReadBool(v);break;case "Enabled":s.Enabled=ReadBool(v);break;}
            }
            s.Shortcut.Validate();return s;
        }
        static bool ReadBool(string value) { if(value=="1")return true;if(value=="0")return false;throw new FormatException("配置中的开关值无效。"); }
        public void Save(string path) {
            Shortcut.Validate();string dir=Path.GetDirectoryName(path);Directory.CreateDirectory(dir);
            string text="Key="+Shortcut.Key+"\r\nCtrl="+(Shortcut.Ctrl?1:0)+"\r\nAlt="+(Shortcut.Alt?1:0)+"\r\nShift="+(Shortcut.Shift?1:0)+"\r\nWin="+(Shortcut.Win?1:0)+"\r\nEnabled="+(Enabled?1:0)+"\r\n";
            text+="CompatibilityMode="+(CompatibilityMode?1:0)+"\r\n";
            if(Shortcut.RecordedKeys!=null){List<string> tokens=new List<string>();foreach(ushort key in Shortcut.RecordedKeys)tokens.Add(key.ToString("X2"));text+="Chord="+String.Join(",",tokens.ToArray())+"\r\n";}
            string temp=path+".tmp";File.WriteAllText(temp,text,new UTF8Encoding(false));
            if(File.Exists(path))File.Replace(temp,path,null);else File.Move(temp,path);
        }
    }

    static class StartupRegistration {
        public const string RunKey=@"Software\Microsoft\Windows\CurrentVersion\Run";
        public const string ValueName="DJIMicKey";
        public static string Command(string executable){string full=Path.GetFullPath(executable);if(full.IndexOf('"')>=0||full.IndexOf('\r')>=0||full.IndexOf('\n')>=0)throw new ArgumentException("启动路径无效。");string command="\""+full+"\" --autostart";if(command.Length>260)throw new ArgumentException("程序路径过长，请先将程序移到较短的固定路径。");return command;}
        public static bool IsEnabled(string executable){return Matches(RunKey,ValueName,executable);}
        internal static bool Matches(string keyPath,string valueName,string executable){using(RegistryKey key=Registry.CurrentUser.OpenSubKey(keyPath,false)){return key!=null&&String.Equals(key.GetValue(valueName) as string,Command(executable),StringComparison.OrdinalIgnoreCase);}}
        public static void SetEnabled(bool enabled,string executable){Write(RunKey,ValueName,enabled,executable);}
        internal static void Write(string keyPath,string valueName,bool enabled,string executable){if(enabled){string command=Command(executable);if(!File.Exists(executable))throw new FileNotFoundException("启动程序不存在。",executable);using(RegistryKey key=Registry.CurrentUser.CreateSubKey(keyPath)){if(key==null)throw new IOException("无法写入当前用户启动项。");key.SetValue(valueName,command,RegistryValueKind.String);}}else{using(RegistryKey key=Registry.CurrentUser.OpenSubKey(keyPath,true)){if(key!=null)key.DeleteValue(valueName,false);}}}
    }

    struct GateResult {public bool Suppress, Trigger; public GateResult(bool suppress,bool trigger){Suppress=suppress;Trigger=trigger;} }
    sealed class MappingGate {
        bool captured; long last=-1000;
        public GateResult Process(bool down, bool eligible, long now) {
            if(down) { if(captured)return new GateResult(true,false);if(!eligible)return new GateResult(false,false);captured=true;return new GateResult(true,false); }
            if(!captured)return new GateResult(false,false);captured=false;
            bool trigger=eligible&&now-last>=250;if(trigger)last=now;return new GateResult(true,trigger);
        }
    }
    sealed class ShortcutRecorder {
        readonly HashSet<ushort> held=new HashSet<ushort>();readonly List<ushort> keys=new List<ushort>();
        public bool Cancelled,Complete;public string Error="";public ShortcutSpec Result;
        public void Process(ushort key,bool down){if(down){if(key==0x1B){Cancelled=true;return;}if(held.Add(key)&&!keys.Contains(key))keys.Add(key);try{if(key==0xAF)throw new ArgumentException("音量加是麦克风的触发键，请使用另一个目标按键。");ShortcutSpec.FromKeys(keys);}catch(ArgumentException ex){Error=ex.Message;}}else{held.Remove(key);if(held.Count==0&&keys.Count>0){Complete=true;if(Error.Length==0)Result=ShortcutSpec.FromKeys(keys);}}}
        public string Display {get{List<string> parts=new List<string>();foreach(ushort k in keys){try{parts.Add(ShortcutSpec.Find(k).Label);}catch(ArgumentException){}}return parts.Count==0?"请按下一个按键或组合键…":String.Join(" + ",parts.ToArray());}}
    }
    sealed class KeyboardMapper : IDisposable {
        Native.HookProc callback;IntPtr hook;readonly MappingGate gate=new MappingGate();readonly object recordLock=new object();
        readonly Stopwatch clock=Stopwatch.StartNew();readonly ManualResetEvent started=new ManualResetEvent(false),healthAck=new ManualResetEvent(false);readonly Thread thread;uint threadId;Exception startupError;
        ShortcutRecorder recorder;readonly HashSet<ushort> swallowed=new HashSet<ushort>();
        readonly Dictionary<string,MappingGate> rawGates=new Dictionary<string,MappingGate>();
        public volatile bool RawSource;
        public void ResetRaw(){lock(recordLock)rawGates.Clear();}
        public void ProcessRawVolume(string source,bool down){bool trigger=false;lock(recordLock){
            if(!RawSource||!Enabled||!Connected||recorder!=null){rawGates.Clear();return;}
            MappingGate rawGate;if(!rawGates.TryGetValue(source,out rawGate)){rawGate=new MappingGate();rawGates[source]=rawGate;}
            GateResult result=rawGate.Process(down,true,clock.ElapsedMilliseconds);trigger=result.Trigger;if(down&&SourcePressed!=null)SourcePressed();
            if(Trace!=null)Trace.Add("RAW DECISION down="+down+" trigger="+trigger+" source="+source);
        }if(trigger&&Pressed!=null)Pressed();}
        public InputTrace Trace;public bool IsRunning {get{return thread!=null&&thread.IsAlive&&hook!=IntPtr.Zero;}}
        public bool ProbeHealth(){healthAck.Reset();Native.Input down=new Native.Input();down.type=1;down.data.keyboard=new Native.KeyboardInput{vk=0xE8,extra=Native.Marker};Native.Input up=down;up.data.keyboard.flags=2;uint sent=Native.SendInput(2,new[]{down,up},Marshal.SizeOf(typeof(Native.Input)));return sent==2&&healthAck.WaitOne(200);}
        public volatile bool Enabled,Connected;public event Action Pressed;public event Action SourcePressed;bool sourceDown;public event Action<string> RecordingProgress;public event Action<ShortcutSpec,string> RecordingFinished;
        public KeyboardMapper(){thread=new Thread(HookLoop);thread.IsBackground=true;thread.Name="DJI media key listener";thread.SetApartmentState(ApartmentState.STA);thread.Start();if(!started.WaitOne(3000)){Dispose();throw new TimeoutException("媒体键监听启动超时。");}if(startupError!=null){Dispose();throw startupError;}}
        void HookLoop(){try{threadId=Native.GetCurrentThreadId();Native.Message message;Native.PeekMessage(out message,IntPtr.Zero,0,0,0);callback=OnHook;hook=Native.SetWindowsHookEx(13,callback,Native.GetModuleHandle(null),0);if(hook==IntPtr.Zero)throw new Win32Exception(Marshal.GetLastWin32Error(),"无法启动媒体键监听。");started.Set();while(Native.GetMessage(out message,IntPtr.Zero,0,0)>0){Native.TranslateMessage(ref message);Native.DispatchMessage(ref message);}}catch(Exception ex){startupError=ex;started.Set();}finally{if(hook!=IntPtr.Zero){Native.UnhookWindowsHookEx(hook);hook=IntPtr.Zero;}GC.KeepAlive(callback);}}
        public static bool IsOwnEvent(Native.Kbd k){return k.extra==Native.Marker;}
        public void StartRecording(){lock(recordLock){recorder=new ShortcutRecorder();}}
        public void CancelRecording(){lock(recordLock){recorder=null;}}
        IntPtr OnHook(int code,IntPtr message,IntPtr data){
            if(code>=0){Native.Kbd k=(Native.Kbd)Marshal.PtrToStructure(data,typeof(Native.Kbd));
                if(IsOwnEvent(k)&&k.vk==0xE8){healthAck.Set();return new IntPtr(1);}
                if(Trace!=null&&Trace.Active&&InputTrace.IsMedia(k.vk))Trace.Add("LOWLEVEL vk="+k.vk.ToString("X")+" scan="+k.scan.ToString("X")+" flags="+k.flags.ToString("X")+" own="+IsOwnEvent(k)+" enabled="+Enabled+" buttonReady="+Connected);
                // DJI consumer controls are injected by Windows (flags 0x11/0x91).
                // Ignore our own marker only; filtering LLKHF_INJECTED drops the actual mic key.
                if(!IsOwnEvent(k)){int msg=message.ToInt32();bool down=msg==0x100||msg==0x104,up=msg==0x101||msg==0x105;ushort key=ShortcutSpec.Normalize(k.vk,k.scan,k.flags);
                    if(down||up){if(k.vk==0xAF&&!RawSource){if(up)sourceDown=false;else if(!sourceDown){sourceDown=true;if(Connected&&SourcePressed!=null)SourcePressed();}}lock(recordLock){if(recorder!=null){if(down)swallowed.Add(key);else swallowed.Remove(key);recorder.Process(key,down);if(recorder.Cancelled){recorder=null;if(RecordingFinished!=null)RecordingFinished(null,"已取消录制。");}else if(recorder.Complete){ShortcutSpec result=recorder.Result;string error=recorder.Error;recorder=null;if(RecordingFinished!=null)RecordingFinished(result,error);}else if(RecordingProgress!=null)RecordingProgress(recorder.Display);return new IntPtr(1);}if(!down&&swallowed.Remove(key))return new IntPtr(1);if(down&&swallowed.Contains(key))return new IntPtr(1);}
                        if(k.vk==0xAF){GateResult result=gate.Process(down,Enabled&&Connected,clock.ElapsedMilliseconds);if(Trace!=null&&Trace.Active)Trace.Add("DECISION suppress="+result.Suppress+" trigger="+(result.Trigger&&!RawSource));if(result.Trigger&&!RawSource&&Pressed!=null)Pressed();if(result.Suppress)return new IntPtr(1);}
                    }
                }
            }return Native.CallNextHookEx(hook,code,message,data);
        }
        public void Dispose(){if(threadId!=0)Native.PostThreadMessage(threadId,0x12,UIntPtr.Zero,IntPtr.Zero);if(thread!=null&&thread.IsAlive&&Thread.CurrentThread!=thread)thread.Join(1000);if(thread==null||!thread.IsAlive){started.Dispose();healthAck.Dispose();}}
    }

    [StructLayout(LayoutKind.Sequential)] struct PropertyKey { public Guid format;public uint id;public PropertyKey(string guid,uint value){format=new Guid(guid);id=value;} }
    [StructLayout(LayoutKind.Explicit,Size=24)] struct PropVariant { [FieldOffset(0)] public ushort type; [FieldOffset(8)]public IntPtr pointer; public string Text {get{return type==31&&pointer!=IntPtr.Zero?Marshal.PtrToStringUni(pointer):"";}} }
    [ComImport,Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")] class MMDeviceEnumerator {}
    [ComImport,Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)] interface IMMDeviceEnumerator {
        [PreserveSig]int EnumAudioEndpoints(int flow,uint mask,out IMMDeviceCollection devices);
        [PreserveSig]int GetDefaultAudioEndpoint(int flow,int role,out IMMDevice device);
        [PreserveSig]int GetDevice([MarshalAs(UnmanagedType.LPWStr)]string id,out IMMDevice device);
        [PreserveSig]int RegisterEndpointNotificationCallback(IntPtr client);
        [PreserveSig]int UnregisterEndpointNotificationCallback(IntPtr client);
    }
    [ComImport,Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)] interface IMMDeviceCollection {
        [PreserveSig]int GetCount(out uint count);[PreserveSig]int Item(uint index,out IMMDevice device);
    }
    [ComImport,Guid("D666063F-1587-4E43-81F1-B948E807363F"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)] interface IMMDevice {
        [PreserveSig]int Activate(ref Guid id,uint context,IntPtr parameters,[MarshalAs(UnmanagedType.IUnknown)]out object value);
        [PreserveSig]int OpenPropertyStore(uint access,out IPropertyStore store);
        [PreserveSig]int GetId([MarshalAs(UnmanagedType.LPWStr)]out string id);
        [PreserveSig]int GetState(out uint state);
    }
    [ComImport,Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)] interface IPropertyStore {
        [PreserveSig]int GetCount(out uint count);[PreserveSig]int GetAt(uint index,out PropertyKey key);
        [PreserveSig]int GetValue(ref PropertyKey key,out PropVariant value);
        [PreserveSig]int SetValue(ref PropertyKey key,ref PropVariant value);[PreserveSig]int Commit();
    }
    [ComImport,Guid("C02216F6-8C67-4B5B-9D00-D008E73E0064"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)] interface IAudioMeterInformation {
        [PreserveSig]int GetPeakValue(out float value);[PreserveSig]int GetMeteringChannelCount(out uint count);
        [PreserveSig]int GetChannelsPeakValues(uint count,IntPtr values);[PreserveSig]int QueryHardwareSupport(out uint mask);
    }
    [ComImport,Guid("1CB9AD4C-DBFA-4C32-B178-C2F568A703B2"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)] interface IAudioClient {
        [PreserveSig]int Initialize(int share,uint flags,long duration,long period,IntPtr format,IntPtr session);
        [PreserveSig]int GetBufferSize(out uint frames);[PreserveSig]int GetStreamLatency(out long latency);[PreserveSig]int GetCurrentPadding(out uint frames);
        [PreserveSig]int IsFormatSupported(int share,IntPtr format,out IntPtr closest);[PreserveSig]int GetMixFormat(out IntPtr format);
        [PreserveSig]int GetDevicePeriod(out long normal,out long minimum);[PreserveSig]int Start();[PreserveSig]int Stop();[PreserveSig]int Reset();
        [PreserveSig]int SetEventHandle(IntPtr handle);[PreserveSig]int GetService(ref Guid id,[MarshalAs(UnmanagedType.IUnknown)]out object value);
    }
    [ComImport,Guid("C8ADBD64-E71E-48A0-A4DE-185C395CD317"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)] interface IAudioCaptureClient {
        [PreserveSig]int GetBuffer(out IntPtr data,out uint frames,out uint flags,out ulong devicePosition,out ulong qpcPosition);
        [PreserveSig]int ReleaseBuffer(uint frames);[PreserveSig]int GetNextPacketSize(out uint frames);
    }
    sealed class DeviceState {
        public bool Hid; public string EndpointId="",Name="",Error="";
        public bool ButtonReady {get{return Hid;}}
        public bool Connected {get{return Hid&&EndpointId.Length>0;}}
    }
    static class DeviceProbe {
        public static DeviceState Scan() {
            DeviceState state=new DeviceState();
            try { state.Hid=HasDjiHid(); FindCapture(state); }catch(Exception ex){state.Error=Native.Error(ex);}
            return state;
        }
        static bool HasDjiHid() {
            uint size=(uint)Marshal.SizeOf(typeof(Native.RawDevice));
            for(int attempt=0;attempt<3;attempt++) {
                uint count=0;if(Native.GetRawInputDeviceList(IntPtr.Zero,ref count,size)==UInt32.MaxValue)throw new Win32Exception();
                if(count==0)return false;if(count>4096)throw new InvalidOperationException("输入设备数量异常。");
                IntPtr memory=Marshal.AllocHGlobal(checked((int)(count*size)));
                try {
                    uint found=Native.GetRawInputDeviceList(memory,ref count,size);if(found==UInt32.MaxValue)continue;
                    for(int i=0;i<found;i++) {
                        Native.RawDevice device=(Native.RawDevice)Marshal.PtrToStructure(IntPtr.Add(memory,i*(int)size),typeof(Native.RawDevice));
                        uint chars=1024;StringBuilder name=new StringBuilder((int)chars);
                        uint result=Native.GetRawInputDeviceInfo(device.handle,0x20000007,name,ref chars);
                        if(result==UInt32.MaxValue)continue;
                        string path=name.ToString();
                        if(path.IndexOf("VID_2CA3",StringComparison.OrdinalIgnoreCase)>=0&&path.IndexOf("PID_4011",StringComparison.OrdinalIgnoreCase)>=0)return true;
                    }
                    return false;
                }finally{Marshal.FreeHGlobal(memory);}
            }
            throw new Win32Exception("设备列表正在变化，请稍后刷新。");
        }
        static void FindCapture(DeviceState state) {
            IMMDeviceEnumerator enumerator=null;IMMDeviceCollection devices=null;
            try {
                enumerator=(IMMDeviceEnumerator)new MMDeviceEnumerator();Marshal.ThrowExceptionForHR(enumerator.EnumAudioEndpoints(1,1,out devices));
                uint count;Marshal.ThrowExceptionForHR(devices.GetCount(out count));
                for(uint i=0;i<count;i++) {
                    IMMDevice device=null;IPropertyStore store=null;
                    try {
                        Marshal.ThrowExceptionForHR(devices.Item(i,out device));Marshal.ThrowExceptionForHR(device.OpenPropertyStore(0,out store));
                        PropertyKey key=new PropertyKey("A45C254E-DF1C-4EFD-8020-67D146A850E0",14);PropVariant value;
                        Marshal.ThrowExceptionForHR(store.GetValue(ref key,out value));string name;
                        try{name=value.Text;}finally{Native.PropVariantClear(ref value);}
                        if(name.IndexOf("Wireless Mic",StringComparison.OrdinalIgnoreCase)<0&&name.IndexOf("DJI Mic",StringComparison.OrdinalIgnoreCase)<0)continue;
                        string id;Marshal.ThrowExceptionForHR(device.GetId(out id));state.Name=name;state.EndpointId=id;return;
                    }finally{Native.Release(store);Native.Release(device);}
                }
            }finally{Native.Release(devices);Native.Release(enumerator);}
        }
    }
    sealed class AudioMonitor : IDisposable {
        IMMDevice device; IAudioClient client; IAudioCaptureClient capture; IAudioMeterInformation meter;
        public string EndpointId {get;private set;}
        public AudioMonitor(string endpoint) {
            IMMDeviceEnumerator enumerator=null;IntPtr format=IntPtr.Zero;
            try {
                enumerator=(IMMDeviceEnumerator)new MMDeviceEnumerator();Marshal.ThrowExceptionForHR(enumerator.GetDevice(endpoint,out device));
                Guid meterId=typeof(IAudioMeterInformation).GUID;object value;Marshal.ThrowExceptionForHR(device.Activate(ref meterId,23,IntPtr.Zero,out value));meter=(IAudioMeterInformation)value;
                Guid clientId=typeof(IAudioClient).GUID;Marshal.ThrowExceptionForHR(device.Activate(ref clientId,23,IntPtr.Zero,out value));client=(IAudioClient)value;
                Marshal.ThrowExceptionForHR(client.GetMixFormat(out format));Marshal.ThrowExceptionForHR(client.Initialize(0,0,2000000,0,format,IntPtr.Zero));
                Guid captureId=typeof(IAudioCaptureClient).GUID;Marshal.ThrowExceptionForHR(client.GetService(ref captureId,out value));capture=(IAudioCaptureClient)value;
                Marshal.ThrowExceptionForHR(client.Start());EndpointId=endpoint;
            }catch{Dispose();throw;}finally{if(format!=IntPtr.Zero)Marshal.FreeCoTaskMem(format);Native.Release(enumerator);}
        }
        public float Poll() {
            uint frames;Marshal.ThrowExceptionForHR(capture.GetNextPacketSize(out frames));
            for(int i=0;frames>0&&i<128;i++) {
                IntPtr data;uint flags,read;ulong position,qpc;
                Marshal.ThrowExceptionForHR(capture.GetBuffer(out data,out read,out flags,out position,out qpc));
                // Discard audio immediately: no managed copy, recording file, or network.
                Marshal.ThrowExceptionForHR(capture.ReleaseBuffer(read));Marshal.ThrowExceptionForHR(capture.GetNextPacketSize(out frames));
            }
            float peak;Marshal.ThrowExceptionForHR(meter.GetPeakValue(out peak));return Math.Max(0,Math.Min(1,peak));
        }
        public void Dispose() { if(client!=null)try{client.Stop();}catch(COMException){} Native.Release(capture);capture=null;Native.Release(meter);meter=null;Native.Release(client);client=null;Native.Release(device);device=null; }
    }
}
