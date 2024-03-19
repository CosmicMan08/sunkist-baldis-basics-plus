using System;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI;
using UnityEngine;
using MTM101BaldAPI.Registers;
using System.Linq;
using System.IO;

namespace SillyBaldiAdditions
{


    [BepInPlugin("cosmicnyan.meow.baldiplus.sillybaldiadd", "Silly Baldi Additions", "1.0.0.0")]
    public class BasePlugin : BaseUnityPlugin
    {
        public static BasePlugin Instance;

        public AssetManager assetMan = new AssetManager();

        void AddAudioFolderToAssetMan(Color subColor, params string[] path)
        {
            string[] paths = Directory.GetFiles(Path.Combine(path));
            for (int i = 0; i < paths.Length; i++)
            {
                assetMan.Add<SoundObject>("Aud_" + Path.GetFileNameWithoutExtension(paths[i]), ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(paths[i]), Path.GetFileNameWithoutExtension(paths[i]), SoundType.Voice, subColor));
            }
        }
        void AddSpriteFolderToAssetMan(string prefix = "", float pixelsPerUnit = 40f, params string[] path)
        {
            string[] paths = Directory.GetFiles(Path.Combine(path));
            for (int i = 0; i < paths.Length; i++)
            {
                assetMan.Add<Sprite>(prefix + Path.GetFileNameWithoutExtension(paths[i]), AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromFile(paths[i]), pixelsPerUnit));
            }
        }

        void RegisterImportant()
        {
            //Debug.Log("register deez");

            Character testEnum = EnumExtensions.ExtendEnum<Character>("Sunkist");
            PosterObject testPoster = ObjectCreators.CreateCharacterPoster(AssetLoader.TextureFromMod(this, "sunkist_poster.png"), "Sunkist", "Baldi's pet dog, he's scatter-brained but loves attention and *will* bark when he sees a student roaming around!");
            
            SunkistDog sunkist = ObjectCreators.CreateNPC<SunkistDog>("Sunkist", testEnum, testPoster, usesHeatMap: false, hasLooker: true);

            sunkist.spriteRenderer[0].gameObject.transform.localPosition += Vector3.up;
            sunkist.spriteRenderer[0].sprite = assetMan.Get<Sprite>("Sunkist");

            sunkist.audMan = sunkist.GetComponent<AudioManager>();
            sunkist.pantAudMan = sunkist.gameObject.AddComponent<PropagatedAudioManager>();
            sunkist.bark = assetMan.Get<SoundObject>("Bark");
            sunkist.panting = assetMan.Get<SoundObject>("Panting");

            assetMan.Add<SunkistDog>("Sunkist", sunkist);
            NPCMetaStorage.Instance.Add(new NPCMetadata(this.Info, new NPC[] { sunkist }, "Sunkist", NPCFlags.Standard));
        }

        void AddNPCs(string floorName, int floorNumber, LevelObject floorObject)
        {
            //Debug.Log("addnpc reached");
            if (floorName.StartsWith("F"))
            {
                floorObject.potentialNPCs.Add(new WeightedNPC() { selection = assetMan.Get<NPC>("Sunkist"), weight = 75 });
                floorObject.MarkAsNeverUnload();
                //Debug.Log("sunkist spawn :3333");
            }
            else if (floorName == "END")
            {
                floorObject.potentialNPCs.Add(new WeightedNPC() { selection = assetMan.Get<NPC>("Sunkist"), weight = 50 });
                floorObject.MarkAsNeverUnload();
            }

        }

        void Awake()
        {
            //Debug.Log("hello chat i am awake");
            Harmony harmony = new Harmony("cosmicnyan.meow.baldiplus.sillybaldiadd");
            harmony.PatchAll();
            assetMan.Add<Texture2D>("sunkist_poster.png", AssetLoader.TextureFromMod(this, "sunkist_poster.png"));
            //Debug.Log(":3");
            assetMan.Add<Texture2D>("Sunkist", AssetLoader.TextureFromMod(this, "sunkist.png"));
            //Debug.Log(":33");
            assetMan.Add<Sprite>("Sunkist", AssetLoader.SpriteFromTexture2D(assetMan.Get<Texture2D>("Sunkist"), 45));
            assetMan.Add<SoundObject>("Bark", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "sunkist bark.wav"), "Sfx_SunBark", SoundType.Voice, Color.yellow));
            assetMan.Add<SoundObject>("Panting", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "sunkist pant.wav"), "Sfx_SunPant", SoundType.Voice, Color.yellow));
            //Debug.Log(":333");
            LoadingEvents.RegisterOnAssetsLoaded(RegisterImportant, false);
            //Debug.Log("owo");
            GeneratorManagement.Register(this, GenerationModType.Addend, AddNPCs);
            //Debug.Log(":3333");
        }
    }
}
