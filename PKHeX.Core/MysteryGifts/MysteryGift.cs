﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace PKHeX.Core
{
    /// <summary>
    /// Mystery Gift Template File
    /// </summary>
    public abstract class MysteryGift : IEncounterable, IMoveset, IGeneration, ILocation
    {
        /// <summary>
        /// Determines whether or not the given length of bytes is valid for a mystery gift.
        /// </summary>
        /// <param name="len">Length, in bytes, of the data of which to determine validity.</param>
        /// <returns>A boolean indicating whether or not the given length is valid for a mystery gift.</returns>
        public static bool IsMysteryGift(long len) => MGSizes.Contains((int)len);

        private static readonly HashSet<int> MGSizes = new HashSet<int>{WC6.SizeFull, WC6.Size, PGF.Size, PGT.Size, PCD.Size };

        /// <summary>
        /// Converts the given data to a <see cref="MysteryGift"/>.
        /// </summary>
        /// <param name="data">Raw data of the mystery gift.</param>
        /// <param name="ext">Extension of the file from which the <paramref name="data"/> was retrieved.</param>
        /// <returns>An instance of <see cref="MysteryGift"/> representing the given data, or null if <paramref name="data"/> or <paramref name="ext"/> is invalid.</returns>
        /// <remarks>This overload differs from <see cref="GetMysteryGift(byte[])"/> by checking the <paramref name="data"/>/<paramref name="ext"/> combo for validity.  If either is invalid, a null reference is returned.</remarks>
        public static MysteryGift GetMysteryGift(byte[] data, string ext)
        {
            if (ext == null)
                return GetMysteryGift(data);

            switch (data.Length)
            {
                case WC7.SizeFull when ext == ".wc7full":
                case WC7.Size when ext == ".wc7":
                    return new WC7(data);
                case WC6.SizeFull when ext == ".wc6full":
                case WC6.Size when ext == ".wc6":
                    return new WC6(data);
                case WR7.Size when ext == ".wr7":
                    return new WR7(data);

                case PGF.Size when ext == ".pgf":
                    return new PGF(data);
                case PGT.Size when ext == ".pgt":
                    return new PGT(data);
                case PCD.Size when ext == ".pcd" || ext == ".wc4":
                    return new PCD(data);
            }

            return null;
        }

        /// <summary>
        /// Converts the given data to a <see cref="MysteryGift"/>.
        /// </summary>
        /// <param name="data">Raw data of the mystery gift.</param>
        /// <returns>An instance of <see cref="MysteryGift"/> representing the given data, or null if <paramref name="data"/> is invalid.</returns>
        public static MysteryGift GetMysteryGift(byte[] data)
        {
            switch (data.Length)
            {
                case WC6.SizeFull:
                    // Check WC7 size collision
                    if (data[0x205] == 0) // 3 * 0x46 for gen6, now only 2.
                        return new WC7(data);
                    return new WC6(data);
                case WC6.Size:
                    // Check year for WC7 size collision
                    if (BitConverter.ToUInt32(data, 0x4C) / 10000 < 2000)
                        return new WC7(data);
                    return new WC6(data);
                case WR7.Size: return new WR7(data);

                case PGF.Size: return new PGF(data);
                case PGT.Size: return new PGT(data);
                case PCD.Size: return new PCD(data);
                default: return null;
            }
        }

        public string Extension => GetType().Name.ToLower();
        public string FileName => $"{CardHeader}.{Extension}";
        public byte[] Data { get; set; }
        public abstract int Format { get; }

        public PKM ConvertToPKM(ITrainerInfo SAV) => ConvertToPKM(SAV, EncounterCriteria.Unrestricted);
        public abstract PKM ConvertToPKM(ITrainerInfo SAV, EncounterCriteria criteria);

        protected abstract bool IsMatchExact(PKM pkm, IEnumerable<DexLevel> vs);
        protected abstract bool IsMatchDeferred(PKM pkm);

        public EncounterMatchRating IsMatch(PKM pkm, IEnumerable<DexLevel> vs)
        {
            if (!IsMatchExact(pkm, vs))
                return EncounterMatchRating.None;
            if (IsMatchDeferred(pkm))
                return EncounterMatchRating.Deferred;
            return EncounterMatchRating.Match;
        }

        /// <summary>
        /// Creates a deep copy of the <see cref="MysteryGift"/> object data.
        /// </summary>
        /// <returns></returns>
        public MysteryGift Clone()
        {
            byte[] data = (byte[])Data.Clone();
            return GetMysteryGift(data);
        }

        /// <summary>
        /// Gets a friendly name for the underlying <see cref="MysteryGift"/> type.
        /// </summary>
        public string Type => GetType().Name;

        /// <summary>
        /// Gets a friendly name for the underlying <see cref="MysteryGift"/> type for the <see cref="IEncounterable"/> interface.
        /// </summary>
        public string Name => $"Event Gift";

        /// <summary>
        /// Gets a friendly name for the underlying <see cref="MysteryGift"/> type for the <see cref="IEncounterable"/> interface.
        /// </summary>
        public string LongName => $"{Name} ({Type})";

        // Properties
        public virtual int Species { get => -1; set { } }
        public abstract bool GiftUsed { get; set; }
        public abstract string CardTitle { get; set; }
        public abstract int CardID { get; set; }

        public abstract bool IsItem { get; set; }
        public abstract int ItemID { get; set; }

        public abstract bool IsPokémon { get; set; }
        public virtual int Quantity { get => 1; set { } }
        public virtual bool Empty => Data.All(z => z == 0);

        public virtual bool IsBP { get => false; set { } }
        public virtual int BP { get => 0; set { } }
        public virtual bool IsBean { get => false; set { } }
        public virtual int Bean { get => 0; set { } }
        public virtual int BeanCount { get => 0; set { } }

        public virtual string CardHeader => (CardID > 0 ? $"Card #: {CardID:0000}" : "N/A") + $" - {CardTitle.Replace('\u3000',' ').Trim()}";

        public override int GetHashCode()
        {
            int hash = 17;
            foreach (var b in Data)
                hash = (hash * 31) + b;
            return hash;
        }

        // Search Properties
        public virtual int[] Moves { get => Array.Empty<int>(); set { } }
        public virtual int[] RelearnMoves { get => Array.Empty<int>(); set { } }
        public virtual int[] IVs { get => null; set { } }
        public virtual bool IsShiny => false;
        public virtual bool IsEgg { get => false; set { } }
        public virtual int HeldItem { get => -1; set { } }
        public virtual int AbilityType { get => -1; set { } }
        public virtual object Content => this;
        public abstract int Gender { get; set; }
        public abstract int Form { get; set; }
        public abstract int TID { get; set; }
        public abstract int SID { get; set; }
        public abstract string OT_Name { get; set; }
        public abstract int Location { get; set; }

        public abstract int Level { get; set; }
        public int LevelMin => Level;
        public int LevelMax => Level;
        public abstract int Ball { get; set; }
        public virtual bool EggEncounter => IsEgg;
        public int Generation { get => Format; set {} }
        public abstract int EggLocation { get; set; }

        public int TrainerID7 => (int)((uint)(TID | (SID << 16)) % 1000000);
        public int TrainerSID7 => (int)((uint)(TID | (SID << 16)) / 1000000);
    }
}
