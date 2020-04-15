﻿// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using Microsoft.Azure.Commands.Network.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Microsoft.WindowsAzure.Commands.Common.CustomAttributes;

namespace Microsoft.Azure.Commands.Network
{
    [Cmdlet("Add", ResourceManager.Common.AzureRMConstants.AzureRMPrefix + "VirtualNetworkSubnetConfig", DefaultParameterSetName = "SetByResource"), OutputType(typeof(PSVirtualNetwork))]
    public class AddAzureVirtualNetworkSubnetConfigCommand : AzureVirtualNetworkSubnetConfigBase
    {
        [Parameter(
            Mandatory = true,
            HelpMessage = "The name of the subnet")]
        [ValidateNotNullOrEmpty]
        public override string Name { get; set; }

        [Parameter(
            Mandatory = true,
            ValueFromPipeline = true,
            HelpMessage = "The virtualNetwork")]
        public PSVirtualNetwork VirtualNetwork { get; set; }

        public override void Execute()
        {
            base.Execute();

            // Verify if the subnet exists in the VirtualNetwork
            var subnet = this.VirtualNetwork.Subnets.SingleOrDefault(resource => string.Equals(resource.Name, this.Name, System.StringComparison.CurrentCultureIgnoreCase));

            if (subnet != null)
            {
                throw new ArgumentException("Subnet with the specified name already exists");
            }

            if (string.Equals(ParameterSetName, Microsoft.Azure.Commands.Network.Properties.Resources.SetByResource))
            {
                if (this.NetworkSecurityGroup != null)
                {
                    this.NetworkSecurityGroupId = this.NetworkSecurityGroup.Id;
                }

                if (this.RouteTable != null)
                {
                    this.RouteTableId = this.RouteTable.Id;
                }

                if (this.InputObject != null)
                {
                    this.ResourceId = this.InputObject.Id;
                }
            }

            subnet = new PSSubnet();

            subnet.Name = this.Name;
            subnet.AddressPrefix = this.AddressPrefix?.ToList();

            if (this.IpAllocations != null)
            {
                foreach (var allocation in this.IpAllocations)
                {
                    subnet.IpAllocations.Add(allocation);
                }
            }

            if (!string.IsNullOrEmpty(this.NetworkSecurityGroupId))
            {
                subnet.NetworkSecurityGroup = new PSNetworkSecurityGroup();
                subnet.NetworkSecurityGroup.Id = this.NetworkSecurityGroupId;
            }

            if (!string.IsNullOrEmpty(this.RouteTableId))
            {
                subnet.RouteTable = new PSRouteTable();
                subnet.RouteTable.Id = this.RouteTableId;
            }

            if (!string.IsNullOrEmpty(this.ResourceId))
            {
                subnet.NatGateway = new PSNatGateway();
                subnet.NatGateway.Id = this.ResourceId;
            }

            if (this.ServiceEndpoint != null)
            {
                subnet.ServiceEndpoints = new List<PSServiceEndpoint>();
                foreach (var item in this.ServiceEndpoint)
                {
                    var service = new PSServiceEndpoint();
                    service.Service = item;
                    subnet.ServiceEndpoints.Add(service);
                }
            }

            if (this.Delegation != null)
            {
                subnet.Delegations = this.Delegation?.ToList();
            }

            subnet.PrivateEndpointNetworkPolicies = this.PrivateEndpointNetworkPoliciesFlag ?? "Enabled";
            subnet.PrivateLinkServiceNetworkPolicies = this.PrivateLinkServiceNetworkPoliciesFlag ?? "Enabled";

            this.VirtualNetwork.Subnets.Add(subnet);

            WriteObject(this.VirtualNetwork);
        }
    }
}
