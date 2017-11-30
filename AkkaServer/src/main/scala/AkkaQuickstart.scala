//#full-example
import GameRoomManager._
import akka.actor.{Actor, ActorLogging, ActorRef, ActorSystem, Props}

import scala.io.StdIn

object AkkaQuickstart extends App {

  // Create the 'helloAkka' actor system
  val system: ActorSystem = ActorSystem("helloAkka")

  try {


    // Create the 'greeter' actors
    /*val monitor: ActorRef =
      system.actorOf(Greeter.props("Howdy", printer), "howdyGreeter")*/
    val manager: ActorRef = system.actorOf(GameRoomManager.props(), "manager")
    manager ! NewRoom(1)
    manager ! AddPlayer(1,1)
    manager ! AddPlayer(1,2)
    manager ! AddPlayer(1,5)
    manager ! PrintPlayer(1)

    //#create-actors

    println(">>> Press ENTER to exit <<<")
    StdIn.readLine()
  } finally {
    system.terminate()
  }
}
//#main-class
//#full-example
