using FunctionalNetworkModel;
using FunctionalNetworkUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NekoPuppet
{
    public class NodeCommandWrapper : ICommand
    {
       // private Predicate<object> _canExecute;
        private Action<object> _execute;

        public NodeCommandWrapper(/*Predicate<object> canExecute,*/ Action<object> execute)
        {
            //_canExecute = canExecute;
            _execute = execute;
        }

        public event EventHandler CanExecuteChanged;
        public event EventHandler ExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }

    /// <summary>
    /// Interaction logic for NodeGraph.xaml
    /// </summary>
    public partial class NodeGraph : UserControl
    {
        Dictionary<string, MenuItem> MenuItems;

        public NodeGraph()
        {
            InitializeComponent();

            MenuItems = new Dictionary<string, MenuItem>();
        }

        public void RegisterNodeType(string type, string Name, IControlNodeFactoryPlugin node)
        {
            string[] NodeMenuStrings = type.Split('/');
            MenuItem lastMenuItem = miCreateNode;
            for (int x = 0; x < NodeMenuStrings.Length; x++)
            {
                string MenuKey = string.Join("/", NodeMenuStrings.Take(x + 1).ToArray());

                if (!MenuItems.ContainsKey(MenuKey))
                {
                    MenuItems.Add(MenuKey, new MenuItem() { Header = NodeMenuStrings[x] });
                    lastMenuItem.Items.Add(MenuItems[MenuKey]);
                }
                lastMenuItem = MenuItems[MenuKey];
            }

            if (lastMenuItem == null) return;

            lastMenuItem.Items.Add(new MenuItem()
            {
                Header = Name,
                Command = new NodeCommandWrapper(p =>
                {
                    Point position = networkControl.PointFromScreen(cmMenu.PointToScreen(new Point(0, 0)));
                    NodeViewModel model = node.CreateNode();
                    model.X = position.X;
                    model.Y = position.Y;
                    if (model != null)
                        CreateNode(model, /*position,*/ true);
                })
            });
        }

        /// <summary>
        /// Convenient accessor for the view-model.
        /// </summary>
        public NodeGraphViewModel ViewModel
        {
            get
            {
                return (NodeGraphViewModel)DataContext;
            }
        }

        /// <summary>
        /// Event raised when the Window has loaded.
        /// </summary>
        private void NodeGraph_Loaded(object sender, RoutedEventArgs e)
        {
            //
            // Display help text for the sample app.
            //
            //HelpTextWindow helpTextWindow = new HelpTextWindow();
            //helpTextWindow.Left = this.Left + this.Width + 5;
            //helpTextWindow.Top = this.Top;
            //helpTextWindow.Owner = this;
            //helpTextWindow.Show();

            //OverviewWindow overviewWindow = new OverviewWindow();
            //overviewWindow.Left = this.Left;
            //overviewWindow.Top = this.Top + this.Height + 5;
            //overviewWindow.Owner = this;
            //overviewWindow.DataContext = this.ViewModel; // Pass the view model onto the overview window.
            //overviewWindow.Show();
        }





        /// <summary>
        /// Event raised when the user has started to drag out a connection.
        /// </summary>
        private void networkControl_ExecutionConnectionDragStarted(object sender, ExecutionConnectionDragStartedEventArgs e)
        {
            var draggedOutConnector = (ExecutionConnectorViewModel)e.ExecutionConnectorDraggedOut;
            var curDragPoint = Mouse.GetPosition(networkControl);

            //
            // Delegate the real work to the view model.
            //
            var connection = this.ViewModel.ExecutionConnectionDragStarted(draggedOutConnector, curDragPoint);

            //
            // Must return the view-model object that represents the connection via the event args.
            // This is so that NetworkView can keep track of the object while it is being dragged.
            //
            e.ExecutionConnection = connection;
        }

        /// <summary>
        /// Event raised, to query for feedback, while the user is dragging a connection.
        /// </summary>
        private void networkControl_QueryExecutionConnectionFeedback(object sender, QueryExecutionConnectionFeedbackEventArgs e)
        {
            var draggedOutExecutionConnector = (ExecutionConnectorViewModel)e.ExecutionConnectorDraggedOut;
            var draggedOverExecutionConnector = (ExecutionConnectorViewModel)e.DraggedOverExecutionConnector;
            object feedbackIndicator = null;
            bool executionConnectionOk = true;

            this.ViewModel.QueryExecutionConnnectionFeedback(draggedOutExecutionConnector, draggedOverExecutionConnector, out feedbackIndicator, out executionConnectionOk);

            //
            // Return the feedback object to NetworkView.
            // The object combined with the data-template for it will be used to create a 'feedback icon' to
            // display (in an adorner) to the user.
            //
            e.FeedbackIndicator = feedbackIndicator;

            //
            // Let NetworkView know if the connection is ok or not ok.
            //
            e.ExecutionConnectionOk = executionConnectionOk;
        }

        /// <summary>
        /// Event raised while the user is dragging a connection.
        /// </summary>
        private void networkControl_ExecutionConnectionDragging(object sender, ExecutionConnectionDraggingEventArgs e)
        {
            Point curDragPoint = Mouse.GetPosition(networkControl);
            var executionConnection = (ExecutionConnectionViewModel)e.ExecutionConnection;
            this.ViewModel.ExecutionConnectionDragging(curDragPoint, executionConnection);
        }

        /// <summary>
        /// Event raised when the user has finished dragging out a connection.
        /// </summary>
        private void networkControl_ExecutionConnectionDragCompleted(object sender, ExecutionConnectionDragCompletedEventArgs e)
        {
            var executionConnectorDraggedOut = (ExecutionConnectorViewModel)e.ExecutionConnectorDraggedOut;
            var executionConnectorDraggedOver = (ExecutionConnectorViewModel)e.ExecutionConnectorDraggedOver;
            var newConnection = (ExecutionConnectionViewModel)e.ExecutionConnection;
            this.ViewModel.ExecutionConnectionDragCompleted(newConnection, executionConnectorDraggedOut, executionConnectorDraggedOver);
        }





        private void networkControl_NodeDoubleClick(object sender, NodeDoubleClickEventArgs e)
        {
            this.ViewModel.NodeDoubleClick(e.nodes);
        }





        /// <summary>
        /// Event raised when the user has started to drag out a connection.
        /// </summary>
        private void networkControl_ConnectionDragStarted(object sender, ConnectionDragStartedEventArgs e)
        {
            var draggedOutConnector = (ConnectorViewModel)e.ConnectorDraggedOut;
            var curDragPoint = Mouse.GetPosition(networkControl);

            //
            // Delegate the real work to the view model.
            //
            var connection = this.ViewModel.ConnectionDragStarted(draggedOutConnector, curDragPoint);

            //
            // Must return the view-model object that represents the connection via the event args.
            // This is so that NetworkView can keep track of the object while it is being dragged.
            //
            e.Connection = connection;
        }

        /// <summary>
        /// Event raised, to query for feedback, while the user is dragging a connection.
        /// </summary>
        private void networkControl_QueryConnectionFeedback(object sender, QueryConnectionFeedbackEventArgs e)
        {
            var draggedOutConnector = (ConnectorViewModel)e.ConnectorDraggedOut;
            var draggedOverConnector = (ConnectorViewModel)e.DraggedOverConnector;
            object feedbackIndicator = null;
            bool connectionOk = true;

            this.ViewModel.QueryConnnectionFeedback(draggedOutConnector, draggedOverConnector, out feedbackIndicator, out connectionOk);

            //
            // Return the feedback object to NetworkView.
            // The object combined with the data-template for it will be used to create a 'feedback icon' to
            // display (in an adorner) to the user.
            //
            e.FeedbackIndicator = feedbackIndicator;

            //
            // Let NetworkView know if the connection is ok or not ok.
            //
            e.ConnectionOk = connectionOk;
        }

        /// <summary>
        /// Event raised while the user is dragging a connection.
        /// </summary>
        private void networkControl_ConnectionDragging(object sender, ConnectionDraggingEventArgs e)
        {
            Point curDragPoint = Mouse.GetPosition(networkControl);
            var connection = (ConnectionViewModel)e.Connection;
            this.ViewModel.ConnectionDragging(curDragPoint, connection);
        }

        /// <summary>
        /// Event raised when the user has finished dragging out a connection.
        /// </summary>
        private void networkControl_ConnectionDragCompleted(object sender, ConnectionDragCompletedEventArgs e)
        {
            var connectorDraggedOut = (ConnectorViewModel)e.ConnectorDraggedOut;
            var connectorDraggedOver = (ConnectorViewModel)e.ConnectorDraggedOver;
            var newConnection = (ConnectionViewModel)e.Connection;
            this.ViewModel.ConnectionDragCompleted(newConnection, connectorDraggedOut, connectorDraggedOver);
        }

        /// <summary>
        /// Event raised to delete the selected node.
        /// </summary>
        private void DeleteSelectedNodes_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var nodesDeleted = this.ViewModel.DeleteSelectedNodes();

            {
                var target = NodeDeleted;
                if (target != null)
                {
                    foreach (var node in nodesDeleted)
                    {
                        target(this, new NodeDeletedEventArgs() { Node = node });
                    }
                }
            }
        }

        /// <summary>
        /// Event raised to create a new node.
        /// </summary>
        //private void CreateNode_Executed(object sender, ExecutedRoutedEventArgs e)
        //{
        //    CreateNode();
        //}

        /// <summary>
        /// Event raised to delete a node.
        /// </summary>
        private void DeleteNode_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var node = (NodeViewModel)e.Parameter;
            this.ViewModel.DeleteNode(node);

            {
                var target = NodeDeleted;
                if (target != null)
                {
                    target(this, new NodeDeletedEventArgs() { Node = node });
                }
            }
        }
        
        /// <summary>
        /// Event raised to delete a connection.
        /// </summary>
        private void DeleteExecutionConnection_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var connection = (ExecutionConnectionViewModel)e.Parameter;
            this.ViewModel.DeleteExecutionConnection(connection);
        }

        /// <summary>
        /// Event raised to delete a connection.
        /// </summary>
        private void DeleteConnection_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var connection = (ConnectionViewModel)e.Parameter;
            this.ViewModel.DeleteConnection(connection);
        }

        /// <summary>
        /// Creates a new node in the network at the current mouse location.
        /// </summary>
        /*private void CreateNode()
        {
            var newNodePosition = Mouse.GetPosition(networkControl);
            this.ViewModel.CreateNode("New Node!", newNodePosition, true);
        }*/

        /// <summary>
        /// Event raised when the size of a node has changed.
        /// </summary>
        private void Node_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //
            // The size of a node, as determined in the UI by the node's data-template,
            // has changed.  Push the size of the node through to the view-model.
            //
            var element = (FrameworkElement)sender;
            var node = (NodeViewModel)element.DataContext;
            node.Size = new Size(element.ActualWidth, element.ActualHeight);
        }

        private void CreateNode(NodeViewModel node, /*Point nodeLocation,*/ bool centerNode)
        {
            //cmMenu.PlacementRectangle.TopLeft
            //var nodeLocation = Mouse.GetPosition(networkControl);
            //var nodeLocation = networkControl.PointFromScreen(cmMenu.PointToScreen(new Point(0, 0)));

            //node.X = nodeLocation.X;
            //node.Y = nodeLocation.Y;

            this.ViewModel.CreateNode(node, centerNode);


            {
                var target = NodeCreated;
                if (target != null)
                {
                    target(this, new NodeCreatedEventArgs() { Node = node });
                }
            }
        }

        public event NodeCreatedEventHandler NodeCreated;
        public delegate void NodeCreatedEventHandler(object sender, NodeCreatedEventArgs e);
        public class NodeCreatedEventArgs : EventArgs
        {
            public NodeViewModel Node;
        }

        public event NodeDeletedEventHandler NodeDeleted;
        public delegate void NodeDeletedEventHandler(object sender, NodeDeletedEventArgs e);
        public class NodeDeletedEventArgs : EventArgs
        {
            public NodeViewModel Node;
        }

    }
}
