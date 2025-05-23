#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include "gtb-probe.h"

static const char* Square_str[64] = {
    "a1","b1","c1","d1","e1","f1","g1","h1",
    "a2","b2","c2","d2","e2","f2","g2","h2",
    "a3","b3","c3","d3","e3","f3","g3","h3",
    "a4","b4","c4","d4","e4","f4","g4","h4",
    "a5","b5","c5","d5","e5","f5","g5","h5",
    "a6","b6","c6","d6","e6","f6","g6","h6",
    "a7","b7","c7","d7","e7","f7","g7","h7",
    "a8","b8","c8","d8","e8","f8","g8","h8"
};
static const char* P_str[] = {
    "--", "P", "N", "B", "R", "Q", "K"
};

const char** paths;

static void dtm_print(unsigned stm, int tb_available, unsigned info, unsigned pliestomate);

extern "C"
{
    struct ProbeResult { int found; unsigned int tbAvailable; unsigned int info; unsigned int pliesToMate; };

    __declspec(dllexport) ProbeResult Probe(unsigned int stm, unsigned int epsquare, unsigned int castling, unsigned int* ws, unsigned int* bs, unsigned char* wp, unsigned char* bp)
    {
        unsigned int info = tb_UNKNOWN;
        unsigned int pliesToMate;

        int tb_available = tb_probe_hard(stm, epsquare, castling, ws, bs, wp, bp, &info, &pliesToMate);

        //dtm_print(stm, tb_available, info, pliesToMate);

        ProbeResult v;
        v.tbAvailable = tb_available;
        v.pliesToMate = pliesToMate;
        v.info = info;
        v.found = tb_available;
        return v;
    }

    __declspec(dllexport) void Init(char* message, const char** pathsToRead, const int numberOfPaths)
    {
        //make sure you've allocated space for the message return!
        int scheme = tb_CP4;
        int verbosity = 1;
        size_t cache_size = 1024 * 1024 * 1024; /* 1 Gb in this example */
        int wdl_fraction = 96;

        paths = tbpaths_init();

        for (int i = 0; i < numberOfPaths; i++)
        {
            printf("Adding %s", pathsToRead[i]);
            if (NULL == paths) printf("Error here... %d\n", __LINE__);
            paths = tbpaths_add(paths, pathsToRead[i]);
        }

        if (NULL == paths) printf("Error here... %d\n", __LINE__);

        char* initinfo = tb_init(verbosity, scheme, paths);
        /* init cache */
        tbcache_init(cache_size, wdl_fraction);
        tbstats_reset();

        if (initinfo != NULL)
        {
            strcpy(message, initinfo);
        }

    }

    __declspec(dllexport) void Close()
    {
        tbcache_done();
        tb_done();
        paths = tbpaths_done(paths);
    }


}

static void
dtm_print(unsigned stm, int tb_available, unsigned info, unsigned pliestomate)
{
    if (tb_available) {

        if (info == tb_DRAW)
            printf("Draw\n");
        else if (info == tb_WMATE && stm == tb_WHITE_TO_MOVE)
            printf("White mates, plies=%u\n", pliestomate);
        else if (info == tb_BMATE && stm == tb_BLACK_TO_MOVE)
            printf("Black mates, plies=%u\n", pliestomate);
        else if (info == tb_WMATE && stm == tb_BLACK_TO_MOVE)
            printf("Black is mated, plies=%u\n", pliestomate);
        else if (info == tb_BMATE && stm == tb_WHITE_TO_MOVE)
            printf("White is mated, plies=%u\n", pliestomate);
        else {
            printf("FATAL ERROR, This should never be reached\n");
            exit(EXIT_FAILURE);
        }
        printf("\n");
    }
    else {
        printf("Tablebase info not available\n\n");
    }
}



