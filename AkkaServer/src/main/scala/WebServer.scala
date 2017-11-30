import AkkaQuickstart.system
import GameRoomManager.{AddPlayer, NewRoom, PrintPlayer}

import scala.util.{Failure, Success}
import akka.pattern.ask
import akka.actor.{Actor, ActorRef, ActorSystem, Props}
import akka.http.scaladsl.Http
import akka.http.scaladsl.model._
import akka.http.scaladsl.server.Directives._
import akka.stream.ActorMaterializer
import akka.util.Timeout

import scala.concurrent.duration._
import scala.collection.mutable.ListBuffer
import scala.concurrent.Future
import scala.io.StdIn

object WebServer {
  def main(args: Array[String]) {
    implicit val system = ActorSystem("my-system")
    implicit val materializer = ActorMaterializer()
    // needed for the future flatMap/onComplete in the end
    implicit val executionContext = system.dispatcher
    implicit val timeout = Timeout(20 seconds)


    // Create the 'greeter' actors
    val manager: ActorRef = system.actorOf(GameRoomManager.props(), "manager")
    val monitor = system.actorOf(Props[SystemMonitor], "system-monitor")

    val route = pathPrefix("new" / IntNumber) { id =>
      manager ! NewRoom(id)
      monitor ! Cmd("New room: " + id + " ")
      complete(HttpEntity(ContentTypes.`text/html(UTF-8)`, "<h1>Ok</h1>"))
    } ~
      pathPrefix("add" / IntNumber / IntNumber) { (room, player) =>
        manager ! AddPlayer(room, player)
        monitor ! Cmd("New Player " + player + " at room " + room + " ")
        complete(HttpEntity(ContentTypes.`text/html(UTF-8)`, "<h1>Ok</h1>"))
      } ~
      pathPrefix("print" / IntNumber) { room =>
        monitor ! Cmd("Requested Print on room " + room + " ")
        val ret = (manager ? PrintPlayer(room)).mapTo[Future[ListBuffer[Int]]].map( _.mapTo[ListBuffer[Int]].map(_.mkString("Players in Room: ",", ","."))).flatten
        onComplete(ret) {
          case Success(value) => complete(HttpEntity(ContentTypes.`text/html(UTF-8)`,value))
          case Failure(e)      => {
            e.printStackTrace()
            complete(HttpEntity(ContentTypes.`text/html(UTF-8)`,e.getMessage))
          }
        }
      } ~
      pathPrefix("movements") {
        val ret = (monitor ? "movements").mapTo[String]
        onComplete(ret){
          case Success(value) => complete(HttpEntity(ContentTypes.`text/html(UTF-8)`,value))
          case Failure(e) => {
            e.printStackTrace()
            complete(HttpEntity(ContentTypes.`text/html(UTF-8)`,e.getMessage))
          }
        }
      }

    val bindingFuture = Http().bindAndHandle(route, "localhost", 8080)

    println(s"Server online at http://localhost:8080/\nPress RETURN to stop...")
    StdIn.readLine() // let it run until user presses return
    bindingFuture
      .flatMap(_.unbind()) // trigger unbinding from the port
      .onComplete(_ => system.terminate()) // and shutdown when done
  }
}
