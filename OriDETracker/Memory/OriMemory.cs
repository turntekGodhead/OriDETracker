﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

namespace OriDE.Memory
{
    public partial class OriMemory
    {
        private static ProgramPointer GameWorld = new ProgramPointer(AutoDeref.Single, new ProgramSignature(PointerVersion.V1, "558BEC53575683EC0C8B7D08B8????????89388B47", 13));
        private static ProgramPointer GameplayCamera = new ProgramPointer(AutoDeref.Single, new ProgramSignature(PointerVersion.V1, "05480000008B08894DE88B4804894DEC8B40088945F08B05", -4));
        private static ProgramPointer WorldEvents = new ProgramPointer(AutoDeref.Single, new ProgramSignature(PointerVersion.V1, "558BEC5783EC048B7D0C83EC0868????????57393FE8????????83C41083EC0868", 33));
        private static ProgramPointer SeinCharacter = new ProgramPointer(AutoDeref.Single, new ProgramSignature(PointerVersion.V1, "558BEC5783EC048B7D08B8????????8938B8????????893883EC0C68", 11));
        private static ProgramPointer ScenesManager = new ProgramPointer(AutoDeref.Single, new ProgramSignature(PointerVersion.V1, "558BEC5783EC148B7D08B8????????893883EC0C57E8????????83C4108B05", 11));
        private static ProgramPointer GameStateMachine = new ProgramPointer(AutoDeref.Single, new ProgramSignature(PointerVersion.V1, "558BEC5783EC148B7D08B8????????8938E8????????83EC0868", 11));
        private static ProgramPointer RainbowDash = new ProgramPointer(AutoDeref.Single, new ProgramSignature(PointerVersion.V1, "EC535783EC108B7D08C687????????000FB605????????85C074", 19));
        private static ProgramPointer GameController = new ProgramPointer(AutoDeref.Single, new ProgramSignature(PointerVersion.V1, "8B05????????83EC086A0050E8????????83C41085C074208B450883EC0C50E8", 2));
        private static ProgramPointer TAS = new ProgramPointer(AutoDeref.Single, new ProgramSignature(PointerVersion.V1, "558BEC53575683EC0CD9EED95DF00FB73D????????83EC0C6A02E8????????83C410D95DF083EC086AFF6A05E8????????83C4108BD883EC0C6A05E8", 17));
        private static ProgramPointer CoreInput = new ProgramPointer(AutoDeref.Single, new ProgramSignature(PointerVersion.V1, "558BEC83EC488B05????????8B40188B40108945B8B8????????8B08894DC08B400489", 22));
        public Process Program { get; set; }
        public bool IsHooked { get; set; } = false;
        private DateTime lastHooked;
        private static Skill[] AllSkills = new Skill[] { Skill.Sein, Skill.WallJump, Skill.ChargeFlame, Skill.Dash, Skill.DoubleJump, Skill.Bash, Skill.Stomp, Skill.Glide, Skill.Climb, Skill.ChargeJump, Skill.Grenade };

        public OriMemory()
        {
            lastHooked = DateTime.MinValue;
        }

        public Dictionary<string, bool> GetEvents()
        {
            Dictionary<string, bool> results = new Dictionary<string, bool>();
            foreach (var pair in events)
            {
                results[pair.Key] = WorldEvents.Read<bool>(Program, pair.Value + 0x40);
            }
            return results;
        }
        public bool GetEvent(string name)
        {
            int offset = events[name];
            return WorldEvents.Read<bool>(Program, offset + 0x40);
        }
        public Dictionary<string, bool> GetKeys()
        {
            Dictionary<string, bool> results = new Dictionary<string, bool>();
            foreach (var pair in keys)
            {
                results[pair.Key] = WorldEvents.Read<bool>(Program, pair.Value);
            }
            return results;
        }
        public bool GetKey(string name)
        {
            int key = keys[name];
            return WorldEvents.Read<bool>(Program, key);
        }
        public Dictionary<string, bool> GetAbilities()
        {
            Dictionary<string, bool> results = new Dictionary<string, bool>();
            foreach (var pair in abilities)
            {
                results[pair.Key] = SeinCharacter.Read<bool>(Program, 0x0, 0x4c, pair.Value * 4, 0x08);
            }
            return results;
        }
        public bool GetAbility(string name)
        {
            int ability = abilities[name];
            return SeinCharacter.Read<bool>(Program, 0x0, 0x4c, ability * 4, 0x08);
        }
        public bool IsEnteringGame()
        {
            return GameController.Read<bool>(Program, 0x0, 0x68) || GameController.Read<bool>(Program, 0x0, 0x69) || SeinCharacter.Read<uint>(Program) == 0 || (GetCurrentLevel() == 0 && GetCurrentENMax() == 3 && GetCurrentHPMax() == 3);
        }
        public bool CanMove()
        {
            return !GameController.Read<bool>(Program, 0x0, 0x7c) && !GameController.Read<bool>(Program, 0x0, 0x7b) && !SeinCharacter.Read<bool>(Program, 0x0, 0x18, 0x38) && !SeinCharacter.Read<bool>(Program, 0x0, 0x18, 0x40);
        }
        public GameState GetGameState()
        {
            return (GameState)GameStateMachine.Read<int>(Program, 0x0, 0x14);
        }
        public int GetAbilityCells()
        {
            return SeinCharacter.Read<int>(Program, 0x0, 0x2c, 0x2c);
        }

        public Dictionary<Skill, bool> GetTrees()
        {
            Skill[] skills = new Skill[] { Skill.Sein, Skill.WallJump, Skill.ChargeFlame, Skill.DoubleJump, Skill.Bash, Skill.Stomp, Skill.Glide, Skill.Climb, Skill.ChargeJump, Skill.Grenade, Skill.Dash };
            Dictionary<Skill, bool> trees = new Dictionary<Skill, bool>();
            int acs = GetAbilityCells();
            for (int i = 0; i <= 10; i++)
            {
               trees.Add(skills[i], ((acs >> (i + 6)) % 2) == 1);
            }
            return trees;
        }

        /* Shards */
        public int WaterVeinShards()
        {
            return (GetAbilityCells() & 0x00060000) >> 17;
        }
        public int GumonSealShards()
        {
            return (GetAbilityCells() & 0x00180000) >> 19;
        }
        public int SunstoneShards()
        {
            return (GetAbilityCells() & 0x00600000) >> 21;
        }

        /* MapStone Progression */
        public int MapStoneProgression()
        {
            return (GetAbilityCells() & 0x07800000) >> 23;
        }

        public int GetSkillPointsAvailable()
        {
            return SeinCharacter.Read<int>(Program, 0x0, 0x38, 0x24);
        }
        public int GetCurrentLevel()
        {
            return SeinCharacter.Read<int>(Program, 0x0, 0x38, 0x28);
        }
        public int GetExperience()
        {
            return SeinCharacter.Read<int>(Program, 0x0, 0x38, 0x2c);
        }
        public int GetCurrentHP()
        {
            return (int)SeinCharacter.Read<float>(Program, 0x0, 0x40, 0x0c, 0x1c);
        }
        public int GetCurrentHPMax()
        {
            return SeinCharacter.Read<int>(Program, 0x0, 0x40, 0x0c, 0x20) / 4;
        }
        public float GetCurrentEN()
        {
            return SeinCharacter.Read<float>(Program, 0x0, 0x3c, 0x20);
        }
        public float GetCurrentENMax()
        {
            return SeinCharacter.Read<float>(Program, 0x0, 0x3c, 0x24);
        }
        public void SetTASCharacter(byte keyCode)
        {
            if (TAS.GetPointer(Program) != IntPtr.Zero)
            {
                TAS.Write<byte>(Program, keyCode);
            }
        }
        public int GetTASState()
        {
            return TAS.Read<int>(Program, -0x1c);
        }
        public string GetTASCurrentInput()
        {
            return TAS.Read(Program, 0x4);
        }
        public string GetTASNextInput()
        {
            return TAS.Read(Program, 0x8);
        }
        public string GetTASExtraInfo()
        {
            return TAS.Read(Program, 0xc);
        }
        public PointF GetTASOriPositon()
        {
            if (!IsHooked) { return new PointF(0, 0); }

            float px = TAS.Read<float>(Program, 0x20);
            float py = TAS.Read<float>(Program, 0x24);
            return new PointF(px, py);
        }
        public bool HasTAS()
        {
            return TAS.GetPointer(Program) != IntPtr.Zero;
        }
        public bool HookProcess()
        {
            IsHooked = Program != null && !Program.HasExited;
            if (!IsHooked && DateTime.Now > lastHooked.AddSeconds(1))
            {
                lastHooked = DateTime.Now;
                Process[] processes = Process.GetProcessesByName("OriDE");
                Program = processes.Length == 0 ? null : processes[0];
                if (Program != null && !Program.HasExited)
                {
                    MemoryReader.Update64Bit(Program);
                    IsHooked = true;
                }
            }

            return IsHooked;
        }
        public string GetPointer(string name)
        {
            switch (name)
            {
                case "GameWorld": return GameWorld.Pointer.ToString("X");
                case "GameplayCamera": return GameplayCamera.Pointer.ToString("X");
                case "WorldEvents": return WorldEvents.Pointer.ToString("X");
                case "SeinCharacter": return SeinCharacter.Pointer.ToString("X");
                case "ScenesManager": return ScenesManager.Pointer.ToString("X");
                case "GameStateMachine": return GameStateMachine.Pointer.ToString("X");
                case "RainbowDash": return RainbowDash.Pointer.ToString("X");
            }
            return string.Empty;
        }
        public void AddLogItems(List<string> items)
        {
            foreach (string key in keys.Keys)
            {
                items.Add(key);
            }
            foreach (string key in events.Keys)
            {
                items.Add(key);
            }
            foreach (string key in abilities.Keys)
            {
                items.Add(key);
            }
        }
        public void Dispose()
        {
            if (Program != null) { this.Program.Dispose(); }
        }

        public static Dictionary<string, int> keys = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            {"Water Vein",   0},
            {"Gumon Seal",   1},
            {"Sunstone",     2},
        };
        public static Dictionary<string, int> events = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            {"Ginso Tree Entered",   0},
            {"Mist Lifted",          1},
            {"Clean Water",          2},
            {"Wind Restored",        3},
            {"Gumo Free",            4},
            {"Spirit Tree Reached",  5},
            {"Warmth Returned",      6},
            {"Darkness Lifted",      7}
        };
        public static Dictionary<string, int> abilities = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            {"Bash",                     5},
            {"Charge Flame",             6},
            {"Wall Jump",                7},
            {"Stomp",                    8},
            {"Double Jump",              9},
            {"Charge Jump",              10},
            {"Magnet",                   11},
            {"Ultra Magnet",             12},
            {"Climb",                    13},
            {"Glide",                    14},
            {"Spirit Flame",             15},
            {"Rapid Fire",               16},
            {"Soul Efficiency",          17},
            {"Water Breath",             18},
            {"Charge Flame Blast",       19},
            {"Charge Flame Burn",        20},
            {"Double Jump Upgrade",      21},
            {"Bash Upgrade",             22},
            {"Ultra Defense",            23},
            {"Health Efficiency",        24},
            {"Sense",                    25},
            {"Stomp Upgrade",            26},
            {"Quick Flame",              27},
            {"Map Markers",              28},
            {"Energy Efficiency",        29},
            {"Health Markers",           30},
            {"Energy Markers",           31},
            {"Ability Markers",          32},
            {"Rekindle",                 33},
            {"Regroup",                  34},
            {"Charge Flame Efficiency",  35},
            {"Ultra Soul Flame",         36},
            {"Soul Flame Efficiency",    37},
            {"Split Flame",              38},
            {"Spark Flame",              39},
            {"Cinder Flame",             40},
            {"Ultra Split Flame",        41},
            {"Light Grenade",            42},
            {"Dash",                     43},
            {"Grenade Upgrade",          44},
            {"Charge Dash",              45},
            {"Air Dash",                 46},
            {"Grenade Efficiency",       47}
        };
    }
    public enum PointerVersion
    {
        V1
    }
    public enum AutoDeref
    {
        None,
        Single,
        Double
    }
    public class ProgramSignature
    {
        public PointerVersion Version { get; set; }
        public string Signature { get; set; }
        public int Offset { get; set; }
        public ProgramSignature(PointerVersion version, string signature, int offset)
        {
            Version = version;
            Signature = signature;
            Offset = offset;
        }
        public override string ToString()
        {
            return Version.ToString() + " - " + Signature;
        }
    }
    public class ProgramPointer
    {
        private int lastID;
        private DateTime lastTry;
        private ProgramSignature[] signatures;
        private int[] offsets;
        public IntPtr Pointer { get; private set; }
        public PointerVersion Version { get; private set; }
        public AutoDeref AutoDeref { get; private set; }

        public ProgramPointer(AutoDeref autoDeref, params ProgramSignature[] signatures)
        {
            AutoDeref = autoDeref;
            this.signatures = signatures;
            lastID = -1;
            lastTry = DateTime.MinValue;
        }
        public ProgramPointer(AutoDeref autoDeref, params int[] offsets)
        {
            AutoDeref = autoDeref;
            this.offsets = offsets;
            lastID = -1;
            lastTry = DateTime.MinValue;
        }

        public T Read<T>(Process program, params int[] offsets) where T : struct
        {
            GetPointer(program);
            return program.Read<T>(Pointer, offsets);
        }
        public string Read(Process program, params int[] offsets)
        {
            GetPointer(program);
            return program.Read((IntPtr)program.Read<uint>(Pointer, offsets));
        }
        public byte[] ReadBytes(Process program, int length, params int[] offsets)
        {
            GetPointer(program);
            return program.Read(Pointer, length, offsets);
        }
        public void Write<T>(Process program, T value, params int[] offsets) where T : struct
        {
            GetPointer(program);
            program.Write<T>(Pointer, value, offsets);
        }
        public void Write(Process program, byte[] value, params int[] offsets)
        {
            GetPointer(program);
            program.Write(Pointer, value, offsets);
        }
        public IntPtr GetPointer(Process program)
        {
            if (program == null)
            {
                Pointer = IntPtr.Zero;
                lastID = -1;
                return Pointer;
            }
            else if (program.Id != lastID)
            {
                Pointer = IntPtr.Zero;
                lastID = program.Id;
            }

            if (Pointer == IntPtr.Zero && DateTime.Now > lastTry.AddSeconds(1))
            {
                lastTry = DateTime.Now;

                Pointer = GetVersionedFunctionPointer(program);
                if (Pointer != IntPtr.Zero)
                {
                    if (AutoDeref != AutoDeref.None)
                    {
                        Pointer = (IntPtr)program.Read<uint>(Pointer);
                        if (AutoDeref == AutoDeref.Double)
                        {
                            if (MemoryReader.is64Bit)
                            {
                                Pointer = (IntPtr)program.Read<ulong>(Pointer);
                            }
                            else
                            {
                                Pointer = (IntPtr)program.Read<uint>(Pointer);
                            }
                        }
                    }
                }
            }
            return Pointer;
        }
        private IntPtr GetVersionedFunctionPointer(Process program)
        {
            if (signatures != null)
            {
                MemorySearcher searcher = new MemorySearcher();
                searcher.MemoryFilter = delegate (MemInfo info) {
                    return (info.State & 0x1000) != 0 && (info.Protect & 0x40) != 0 && (info.Protect & 0x100) == 0;
                };
                for (int i = 0; i < signatures.Length; i++)
                {
                    ProgramSignature signature = signatures[i];

                    IntPtr ptr = searcher.FindSignature(program, signature.Signature);
                    if (ptr != IntPtr.Zero)
                    {
                        Version = signature.Version;
                        return ptr + signature.Offset;
                    }
                }
            }
            else
            {
                IntPtr ptr = (IntPtr)program.Read<uint>(program.MainModule.BaseAddress, offsets);
                if (ptr != IntPtr.Zero)
                {
                    return ptr;
                }
            }

            return IntPtr.Zero;
        }
    }
}