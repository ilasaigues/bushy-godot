namespace BushyCore
{
    public static class CollisionLayerConsts
    {
        public const int PLAYER = 1;
        public const int PLAYER_LAYER = 1 << PLAYER;
        public const int TILE_MAP = 2;
        public const int TILE_MAP_LAYER = 1 << TILE_MAP;
        public const int HEDGE = 3;
        public const int HEDGE_LAYER = 1 << HEDGE;
        public const int PLATFORM = 6;
        public const int PLATFORM_LAYER = 1 << PLATFORM;

        public const int SOLID_LAYERS = TILE_MAP_LAYER | HEDGE_LAYER | PLATFORM_LAYER;
    }
}