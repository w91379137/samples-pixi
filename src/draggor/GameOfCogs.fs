module GameOfCogs
open Types 
open Fable.Import
open Fable.Import.Pixi
open Fable.Import.Pixi.Particles
open Fable.Pixi
open Fable.Core
open Fable.Core.JsInterop
open Fable.Import.Animejs



let DisplayParticles model delta = 
  model.Emitters
    |> Seq.iter( fun emitter -> emitter.update (delta * 0.01) )
  
let addEmitter x y config = 
  let texture = Assets.getTexture "particle"
  if texture.IsSome then        
    // create our emitter 
    let container = Layers.get "emitter"
    if container.IsNone then
      failwith "unknown layer emitter"
    else 
      let emitter = PIXI.particles.Emitter( container.Value, !![|texture.Value|], config )
      emitter.updateOwnerPos(x,y)

      // start emitting particles
      emitter.emit <- true

      Some emitter
  else 
    None
      

let greatAnim x y = 

  let container = Layers.get "top"
  match container with 
    | Some c -> 
      let help = Assets.getTexture "great"
      if help.IsSome then 
        let sprite = 
          PIXI.Sprite help.Value
          |> c.addChild
        
        sprite._anchor.set 0.5

        let scale : PIXI.Point = !!sprite.scale
        scale.x <- 0.0
        scale.y <- 0.0

        let position : PIXI.Point = !!sprite.position
        position.x <- x
        position.y <- y

                
        let timelineOptions = 
          jsOptions<anime.AnimeTimelineInstance>( fun o -> 
            o.complete <- fun _ -> sprite.parent.removeChild(sprite) |> ignore
          )

        let prepareAnimation scale= 
          jsOptions<anime.AnimeAnimParams> (fun o ->
            o.elasticity <- !!100.
            o.duration <- !!500.
            o.targets <- !!sprite.scale
            o.Item("x") <- scale
            o.Item("y") <- scale
            o.complete <- Some (fun _ -> printfn "done")
          )
        
        // create our tweening timeline
        let timeline = anime.Globals.timeline(!!timelineOptions)
        
        // prepare our animations
        [
          prepareAnimation 1.0 // scale in 
          prepareAnimation 0.0 // scale out
        ] |> Seq.iter( fun options -> timeline.add options |> ignore ) 
      
    | None -> failwith "no layer top"

      
let handleMessage model (msg:Msg option) = 

    if msg.IsSome then 
      match msg.Value with 
      | OnMove (cog,pointerId) -> 
        let pos : PIXI.Point = !!cog.position
        let mutable score = model.Score
        let mutable found = model.Found
        let mutable emitters = model.Emitters
        let updatedTargets = 
          model.Targets
            |> Seq.mapi( fun i target -> 
              if not target.Data.IsFound then 
                let position : PIXI.Point = !!target.position
                
                // very simple distance text
                let a = position.x - pos.x
                let b = position.y - pos.y
                let distance = JS.Math.sqrt( a*a + b*b)

                // look if we are in close vicinity of a potential target
                let checkRadius = Cog.cogWidth * (Cog.scaleFactor cog.Data.Size) * 0.2
                if distance < checkRadius then
                  if cog.Data.Size = target.Data.Size then 

                    // ok our cog has been placed at the right place
                    // store index for faster animation renders              
                    found <- Array.append found [|i|]
                    
                    // restore cog to initial position
                    Cog.onDragEnd cog () |> ignore
                    
                    // display the target cog
                    Cog.show target |> ignore                      
                    
                    // update score
                    score <- score + 1

                    // add new particle system at the right place
                    // given the way our cog is turning
                    let isLeft = (i % 2 = 0)
                    let config = 
                      if isLeft then 
                        Assets.getObj "leftConfig" 
                      else 
                        Assets.getObj "rightConfig"

                    if config.IsSome then 
                      let config = config.Value
                      let x = 
                        if isLeft then 
                          position.x - target.width * 0.4
                        else
                          position.x + target.width * 0.4

                      let y = 
                          position.y - target.height * 0.3
                        
                      let newEmitter = (addEmitter x y config)
                      if newEmitter.IsSome then 
                        emitters <- Array.append emitters [|newEmitter.Value|]

                      // Great feedback anim
                      greatAnim position.x (position.y - 50.)                                
                //target
              target
            )
            |> Seq.toArray

        printfn "%i" (emitters |> Seq.length)
        model.Targets <- updatedTargets 
        model.Score <- score 
        model.Found <- found 
        model.Emitters <- emitters
    
          (*
      // check if the game's finished
      if model.Score >=  model.Goal then 
        { model with 
//                    State = GameOver;
            Targets = updatedTargets 
            Found = foundCogs
            Score= score
        }
      else 
        { model with 
            Targets = updatedTargets 
            Found = foundCogs
            Score= score              
        }
        *)

  
let Update model stage (renderer:PIXI.WebGLRenderer) delta =

  // update our particles
  DisplayParticles model delta
 
  match model.State with 
    | Init ->           
        model.Goal <- Cog.cogSizes.Length // the cogs to place correctly in order to win
        model.State <- PlaceHelp

        ScreenKind.GameOfCogs      
    | PlaceHelp -> 
      
      let container = Layers.get "ui"
      match container with 
        | Some c -> 
          let help = Assets.getTexture "help1"
          if help.IsSome then 
            let sprite = 
              PIXI.Sprite help.Value
              |> c.addChild

            sprite._anchor.set(0.5) 
            let position : PIXI.Point = !!sprite.position
            position.x <- renderer.width * 0.65
            position.y <- renderer.height * 0.75

          let help = Assets.getTexture "help2"
          if help.IsSome then 
            let sprite = 
              PIXI.Sprite help.Value
              |> c.addChild

            sprite._anchor.set(0.5) 
            let position : PIXI.Point = !!sprite.position
            position.x <- renderer.width * 0.70
            position.y <- renderer.height * 0.3

        | None -> failwith "ui layer not found"

      model.State <- PlaceCogs

      ScreenKind.GameOfCogs      
    | PlaceCogs -> 

      let container = Layers.get "cogs"
      match container with 
        | Some c -> 

          // create our cogs and center them!
          let targets = 
            
            // create our cogs
            // they have to fit in the given space
            let maxWidth = renderer.width * 0.8
            let targets,(totalWidth,totalHeight) 
              = Cog.fitCogInSpace model 0 (0.,0.) (0.,0.) None [] maxWidth Cog.cogSizes
                      
            // center our cogs
            let xMargin = (renderer.width - totalWidth) * 0.5
            let yMargin = totalHeight * 0.5
            targets 
              |> Seq.map ( 
                  (Cog.placeMarker xMargin yMargin (renderer.height*0.5)) 
                    >> c.addChild
                  )
               |> Seq.toArray
         
          model.Targets <- targets
          model.State <- PlaceDock
        | None -> failwith "unknown container cogs"
      
      ScreenKind.GameOfCogs      
    | PlaceDock -> // prepare our 4 base cogs

      let container = Layers.get "dock"
      match container with 
        | Some c -> 
          let cogs =  
            Dock.prepare c stage renderer 
            |> Seq.map( fun cog -> 
                cog.interactive <- true
                cog.buttonMode <- true        
                cog
                  |> attachEvent Pointerdown (Cog.onDragStart cog)
                  |> attachEvent Pointerup (Cog.onDragEnd cog)
                  |> attachEvent Pointermove (Cog.onDragMove (handleMessage model) stage cog)
                  |> c.addChild
                  |> Cog.backTo
            )
            |> Seq.toArray

          model.Cogs <- cogs
          model.State <- Play
        | None -> failwith "unknown container dock"

      ScreenKind.GameOfCogs
    | Play -> 


      // Animations
      if model.Score > 0 then
        // make our cogs turn
        model.Found
          |> Seq.iter( fun i ->
            let target = model.Targets.[i] 
            // animate all the cogs that have been found
            // make sure the next cog will turn on the opposite direction
            let way = if i % 2 = 0 then 1. else -1.0
            // smaller cogs run faster
            let speed = Cog.rotation * (1.25 - (Cog.scaleFactor target.Data.Size))
            target.rotation <- target.rotation + way * speed
          )              
      
      ScreenKind.GameOfCogs

    | DoNothing -> ScreenKind.GameOfCogs
