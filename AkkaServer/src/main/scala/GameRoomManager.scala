import GameRoom.{NewPlayer, PrintPlayers}
import GameRoomManager.{AddPlayer, DeleteRoom, NewRoom, PrintPlayer}
import akka.actor.{Actor, ActorRef, ActorSystem, Props}
import akka.pattern.ask
import akka.util.Timeout
import scala.concurrent.duration._
import scala.collection.mutable.ListBuffer

class GameRoomManager extends Actor {
  var rooms = Map[Int,ActorRef]()
  implicit val system = ActorSystem("my-system")
  implicit val timeout = Timeout(5 seconds)

  override def receive: Receive = {
    case NewRoom(id)  => rooms += (id -> system.actorOf(GameRoom.props(id)))

    case DeleteRoom(id) => rooms -= id

    case AddPlayer(room,player)  => rooms(room) ! NewPlayer(player)

    case PrintPlayer(id) => sender ! (rooms(id) ? PrintPlayers)
  }
}
object GameRoomManager {
  def props() = Props(new GameRoomManager)

  final case class NewRoom(id: Int)
  final case class DeleteRoom(id: Int)
  final case class AddPlayer(playerId: Int, roomId: Int)
  final case class PrintPlayer(id: Int)
}
