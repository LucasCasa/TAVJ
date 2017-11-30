import GameRoom.{NewPlayer, PrintPlayers}
import akka.actor.{Actor, Props}

import scala.collection.mutable.ListBuffer

class GameRoom (val id: Int) extends Actor{
  var players = ListBuffer[Int]()

  override def receive: Receive = {

    case NewPlayer(id) => players += id

    case PrintPlayers =>  sender ! players
  }
}
object GameRoom {
  //#greeter-messages
  def props(id: Int) = Props(new GameRoom(id))

  final case class NewPlayer(id: Int)
  final case class PrintPlayers()
}

