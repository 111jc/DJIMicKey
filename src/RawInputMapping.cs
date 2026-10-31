using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace DjiMicKey {
    // One Raw Input owner per process. Diagnostics observe this same persistent
    // listener; starting/stopping a report must not unregister the mapping source.
    sealed class RawControlMapper : NativeWindow, IDisposable {
        [StructLayout(LayoutKind.Sequential)] struct Registration { public ushort page,usage;public uint flags;public IntPtr target; }
        [DllImport("user32.dll",SetLastError=true)] static extern bool RegisterRawInputDevices(Registration[] items,uint count,uint size);
        [DllImport("user32.dll",SetLastError=true)] static extern uint GetRawInputData(IntPtr raw,uint command,IntPtr buffer,ref uint size,uint header);
        [DllImport("user32.dll",EntryPoint="GetRawInputDeviceInfoW",SetLastError=true)] static extern uint DeviceInfo(IntPtr device,uint command,IntPtr data,ref uint size);
        [DllImport("hid.dll")] static extern int HidP_GetUsages(int type,ushort page,ushort collection,[Out] ushort[] usages,ref uint count,IntPtr preparsed,byte[] report,uint length);
        sealed class Source : IDisposable {
            public string Path;public IntPtr Data;public readonly Dictionary<byte,bool> Held=new Dictionary<byte,bool>();
            public void Dispose(){if(Data!=IntPtr.Zero){Marshal.FreeHGlobal(Data);Data=IntPtr.Zero;}}
        }
        readonly InputTrace trace;readonly Dictionary<IntPtr,Source> sources=new Dictionary<IntPtr,Source>();bool registered;
        public event Action<string,bool> VolumeChanged;
        public event Action SourceReset;
        public string LastError="";
        public RawControlMapper(InputTrace trace){this.trace=trace;try{
            CreateHandle(new CreateParams{Caption="DJI receiver input",Parent=new IntPtr(-3)});
            Registration r=new Registration{page=0x0C,usage=1,flags=0x100|0x2000,target=Handle};
            if(!RegisterRawInputDevices(new[]{r},1,(uint)Marshal.SizeOf(typeof(Registration))))throw new Win32Exception();registered=true;
        }catch{Dispose();throw;}}
        public static bool IsTargetPath(string path){return path!=null&&path.IndexOf("VID_2CA3&PID_4011",StringComparison.OrdinalIgnoreCase)>=0;}
        public static List<byte[]> SplitReports(byte[] payload){
            if(payload==null||payload.Length<8)throw new ArgumentException("Truncated RAWHID header");
            uint size=BitConverter.ToUInt32(payload,0),count=BitConverter.ToUInt32(payload,4);
            if(size==0||count==0||(ulong)size*count>(ulong)(payload.Length-8))throw new ArgumentException("Invalid RAWHID report count/length");
            List<byte[]> reports=new List<byte[]>();for(uint i=0;i<count;i++){byte[] report=new byte[(int)size];Array.Copy(payload,8+checked((int)(i*size)),report,0,report.Length);reports.Add(report);}return reports;
        }
        Source GetSource(IntPtr device){Source source;if(sources.TryGetValue(device,out source))return source;
            uint length=2048;StringBuilder path=new StringBuilder((int)length);
            if(Native.GetRawInputDeviceInfo(device,0x20000007,path,ref length)==UInt32.MaxValue)throw new Win32Exception();
            if(!IsTargetPath(path.ToString())){sources[device]=null;return null;}
            IntPtr info=Marshal.AllocHGlobal(32);try{Marshal.WriteInt32(info,32);uint size=32;
                if(DeviceInfo(device,0x2000000B,info,ref size)==UInt32.MaxValue)throw new Win32Exception();
                if(Marshal.ReadInt32(info,4)!=2||Marshal.ReadInt16(info,20)!=0x0C||Marshal.ReadInt16(info,22)!=1){sources[device]=null;return null;}
            }finally{Marshal.FreeHGlobal(info);}
            uint bytes=0;if(DeviceInfo(device,0x20000005,IntPtr.Zero,ref bytes)==UInt32.MaxValue||bytes==0||bytes>1048576)throw new Win32Exception("Cannot query DJI HID descriptor");
            source=new Source{Path=path.ToString(),Data=Marshal.AllocHGlobal((int)bytes)};
            try{if(DeviceInfo(device,0x20000005,source.Data,ref bytes)==UInt32.MaxValue)throw new Win32Exception();sources[device]=source;return source;}catch{source.Dispose();throw;}
        }
        public void Describe(){trace.Add("RAW mapping active=True; source=VID_2CA3 PID_4011 Consumer; parser=HidP_GetUsages; keyboard hook suppresses only");}
        protected override void WndProc(ref Message message){try{
            if(message.Msg==0xFE){Source gone;if(sources.TryGetValue(message.LParam,out gone)){if(gone!=null)gone.Dispose();sources.Remove(message.LParam);if(gone!=null&&SourceReset!=null)SourceReset();}trace.Add("RAW device-change="+message.WParam);}
            if(message.Msg==0xFF){uint size=0,header=(uint)(8+2*IntPtr.Size);
                if(GetRawInputData(message.LParam,0x10000003,IntPtr.Zero,ref size,header)==UInt32.MaxValue||size<=header||size>1048576)return;
                IntPtr memory=Marshal.AllocHGlobal((int)size);try{
                    if(GetRawInputData(message.LParam,0x10000003,memory,ref size,header)==UInt32.MaxValue||Marshal.ReadInt32(memory)!=2)return;
                    Source source=GetSource(Marshal.ReadIntPtr(memory,8));if(source==null)return;
                    byte[] payload=new byte[(int)size-(int)header];Marshal.Copy(IntPtr.Add(memory,(int)header),payload,0,payload.Length);
                    trace.Add("RAW DJI HID bytes="+BitConverter.ToString(payload,0,Math.Min(payload.Length,64))+" source="+source.Path);
                    foreach(byte[] report in SplitReports(payload)){
                        ushort[] usages=new ushort[256];uint count=(uint)usages.Length;int status=HidP_GetUsages(0,0x0C,0,usages,ref count,source.Data,report,(uint)report.Length);
                        if(status!=0x110000){LastError="HID parser 0x"+status.ToString("X8");trace.Add("RAW decode error="+LastError+" report="+BitConverter.ToString(report));continue;}
                        bool down=false;for(int i=0;i<count;i++)if(usages[i]==0xE9)down=true;
                        bool held;source.Held.TryGetValue(report[0],out held);source.Held[report[0]]=down;
                        trace.Add("RAW decoded reportId="+report[0]+" volumeUp="+down);
                        if(down!=held&&VolumeChanged!=null)VolumeChanged(source.Path+"/"+report[0],down);
                    }
                }finally{Marshal.FreeHGlobal(memory);}
            }
        }catch(Exception ex){LastError=Native.Error(ex);trace.Add("RAW mapping error="+LastError);}finally{base.WndProc(ref message);}}
        public void Dispose(){if(registered){Registration r=new Registration{page=0x0C,usage=1,flags=1,target=IntPtr.Zero};RegisterRawInputDevices(new[]{r},1,(uint)Marshal.SizeOf(typeof(Registration)));registered=false;}foreach(Source source in sources.Values)if(source!=null)source.Dispose();sources.Clear();if(Handle!=IntPtr.Zero)DestroyHandle();}
    }
}
