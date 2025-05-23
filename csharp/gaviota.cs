using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

public class CastlingException : Exception
{
    public CastlingException(string message) : base(message)
    {
    }
    public CastlingException(string message, int move) : base(message)
    {

    }
}

public static class Gaviota
{
    public struct ProbeResult
    {
        public uint Found;
        public uint TbAvailable;
        public uint Info;
        public uint PliesToMate;
    }

    public enum Side
    {
        White = 0,
        Black
    }

    private enum PieceType
    {
        Pawn = 1,
        Knight,
        Bishop,
        Rook,
        Queen,
        King
    }

    public enum MateValues
    {
        Draw = 0,
        WhiteMate = 1,
        BlackMate = 2,
    }

    private enum Castling
    {
        NOCASTLE = 0,
        WOO = 8,
        WOOO = 4,
        BOO = 2,
        BOOO = 1
    };

    const uint tb_NOSQUARE = 64;
    const char tb_NOPIECE = (char)0;

    [DllImport("tbprobe.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Init")]
    private static extern int _Init(StringBuilder message, [In] string[] paths, int pathCount);

    [DllImport("tbprobe.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Probe")]
    private static extern ProbeResult _Probe(uint stm, uint epsquare, uint castling, [In] uint[] ws, [In] uint[] bs, [In] char[] wp, [In] char[] bp);

    [DllImport("tbprobe.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Close")]
    private static extern void _Close();

    private static uint[] a8toa1 = new uint[64];

    public static string Init(string[] paths)
    {
        for (uint i = 0; i < 64; i++)
        {
            uint strow = (i % 8);
            uint stcol = 7 - (i / 8);
            uint newId = (stcol * 8) + strow;
            a8toa1[i] = newId;
        }

        var message = new StringBuilder(512);

        var newPaths = new List<string>(paths);

        _Init(message, newPaths.ToArray(), newPaths.Count);
        return message.ToString();

    }

    public static void Close()
    {
        _Close();
    }
    public static (ProbeResult, Side) Probe(string fen)
    {
        //adapted from egtb_probe, but added castling (which I don't think gaviota uses)
        //also does en passant correctly
        //and improved a bit

        Castling castling = Castling.NOCASTLE;

        var whitePieceSquares = new List<uint>();
        var blackPieceSquares = new List<uint>();
        var whiteTypesSquares = new List<char>();
        var blackTypesSquares = new List<char>();

        byte index = 0;
        byte spc = 0;
        byte spacers = 0;

        string ep = "";

        Side whosTurn = Side.White;
        uint enPassantSquare = tb_NOSQUARE;

        foreach (char c in fen)
        {
            if (index < 64 && spc == 0)
            {
                if (c == '1' && index < 63)
                {
                    index++;
                }
                else if (c == '2' && index < 62)
                {
                    index += 2;
                }
                else if (c == '3' && index < 61)
                {
                    index += 3;
                }
                else if (c == '4' && index < 60)
                {
                    index += 4;
                }
                else if (c == '5' && index < 59)
                {
                    index += 5;
                }
                else if (c == '6' && index < 58)
                {
                    index += 6;
                }
                else if (c == '7' && index < 57)
                {
                    index += 7;
                }
                else if (c == '8' && index < 56)
                {
                    index += 8;
                }
                else if (c == 'P')
                {
                    whitePieceSquares.Add(a8toa1[index]);
                    whiteTypesSquares.Add((char)PieceType.Pawn);
                    index++;
                }
                else if (c == 'N')
                {
                    whitePieceSquares.Add(a8toa1[index]);
                    whiteTypesSquares.Add((char)PieceType.Knight);
                    index++;
                }
                else if (c == 'B')
                {
                    whitePieceSquares.Add(a8toa1[index]);
                    whiteTypesSquares.Add((char)PieceType.Bishop);
                    index++;
                }
                else if (c == 'R')
                {
                    whitePieceSquares.Add(a8toa1[index]);
                    whiteTypesSquares.Add((char)PieceType.Rook);
                    index++;
                }
                else if (c == 'Q')
                {
                    whitePieceSquares.Add(a8toa1[index]);
                    whiteTypesSquares.Add((char)PieceType.Queen);
                    index++;
                }
                else if (c == 'K')
                {
                    whitePieceSquares.Add(a8toa1[index]);
                    whiteTypesSquares.Add((char)PieceType.King);
                    index++;
                }
                else if (c == 'p')
                {
                    blackPieceSquares.Add(a8toa1[index]);
                    blackTypesSquares.Add((char)PieceType.Pawn);
                    index++;
                }
                else if (c == 'n')
                {
                    blackPieceSquares.Add(a8toa1[index]);
                    blackTypesSquares.Add((char)PieceType.Knight);
                    index++;
                }
                else if (c == 'b')
                {
                    blackPieceSquares.Add(a8toa1[index]);
                    blackTypesSquares.Add((char)PieceType.Bishop);
                    index++;
                }
                else if (c == 'r')
                {
                    blackPieceSquares.Add(a8toa1[index]);
                    blackTypesSquares.Add((char)PieceType.Rook);
                    index++;
                }
                else if (c == 'q')
                {
                    blackPieceSquares.Add(a8toa1[index]);
                    blackTypesSquares.Add((char)PieceType.Queen);
                    index++;
                }
                else if (c == 'k')
                {
                    blackPieceSquares.Add(a8toa1[index]);
                    blackTypesSquares.Add((char)PieceType.King);
                    index++;
                }
                else if (c == '/')
                {
                    continue;
                }
                else if (c == ' ')
                {
                    spc++;
                }
            }
            else
            {
                if ((c == 'w') && (spacers < 2))
                {
                    whosTurn = Side.White;
                }
                else if ((c == 'b') && (spacers < 2))
                {
                    whosTurn = Side.Black;
                }
                else if (spacers == 2)
                {
                    switch (c)
                    {
                        case 'K':
                            castling |= Castling.WOO;
                            break;
                        case 'Q':
                            castling |= Castling.WOOO;
                            break;
                        case 'k':
                            castling |= Castling.BOO;
                            break;
                        case 'q':
                            castling |= Castling.BOOO;
                            break;
                    }
                }
                else if (spacers == 3)
                {
                    ep += c;
                }
            }

            if (c == ' ')
            {
                spacers++;
            }
        }

        ep = ep.Trim();

        if (ep != "-")
        {
            if (ep.Length == 2)
            {
                if (ep[1] == '3')
                {
                    enPassantSquare = (uint)16 + ep[0] - 'a';
                }
                else if (ep[1] == '6')
                {
                    enPassantSquare = (uint)40 + ep[0] - 'a';
                }
                else
                {
                    throw new Exception("Invalid en passant square");
                }
            }
            else if (ep.Length != 0)
            {
                throw new Exception("Invalid en passant square");
            }
        }

        var ws = whitePieceSquares.ToArray();
        Array.Resize(ref ws, 17);

        var bs = blackPieceSquares.ToArray();
        Array.Resize(ref bs, 17);

        var wp = whiteTypesSquares.ToArray();
        Array.Resize(ref wp, 17);

        var bp = blackTypesSquares.ToArray();
        Array.Resize(ref bp, 17);

        var wLength = whitePieceSquares.Count;
        var bLength = blackPieceSquares.Count;

        ws[wLength] = tb_NOSQUARE;
        wp[wLength] = tb_NOPIECE;

        bs[bLength] = tb_NOSQUARE;
        bp[bLength] = tb_NOPIECE;

        if (castling != Castling.NOCASTLE)
        {
            throw new CastlingException("The Gaviota table bases do not include castling");
        }

        var result = _Probe((uint)whosTurn, enPassantSquare, (uint)castling, ws, bs, wp, bp);

        if (result.Found != 1)
        {
            System.Diagnostics.Debugger.Break();
        }
        return (result, whosTurn);

    }

}

