# CDS.infrastructure — Proposed Network Design

**Company:** Kvist & Bjælke ApS  
**Scope:** This is a theoretical proposal for the company’s network, not an implementation or test report.

## 1. Company requirements

The design considers the company’s current devices and room for future growth.

| Device | Quantity | Purpose |
|---|---:|---|
| Desktop computers | 15 | Administration and office work |
| Laptops | 10 | Employees working from different locations |
| Network printers | 3 | Shared printing |
| Servers | 3 | Internal systems and applications |
| IP phones | 12 | Network-based telephony |
| Wireless access points | 4 | Employee and guest wireless access |
| Surveillance cameras | 6 | IP-based video surveillance |
| Network equipment | As required | Switching, routing, and connectivity |
| Guest devices | Variable | Visitors’ smartphones and computers |

## 2. Proposed network architecture

The proposal uses the two VLANs specified in the assignment: VLAN 570 for clients and VLAN 571 for servers. Keeping only these two segments makes the design straightforward, while separating users from the systems hosting applications.

| VLAN | Network | Devices and purpose |
|---|---|---|
| **570 — Client** | `10.5.70.0/24` | Employee computers and laptops. For this simplified design, printers, phones, cameras, access-point management, and guest devices also use this VLAN. |
| **571 — Server** | `10.5.71.0/24` | The company’s three servers and hosted applications. |

Placing guest and other device types in the client VLAN reduces isolation between them. Separate guest, voice, printer, camera, and management VLANs could improve separation in a future design but are outside this proposal’s scope.

## 3. IP addressing plan

The assignment specifies both `/24` networks. The gateway addresses and address allocations below are proposed values.

| Property | VLAN 570 — Client | VLAN 571 — Server |
|---|---|---|
| Network / CIDR | `10.5.70.0/24` | `10.5.71.0/24` |
| Subnet mask | `255.255.255.0` | `255.255.255.0` |
| Proposed default gateway | `10.5.70.1` | `10.5.71.1` |
| Usable host range | `10.5.70.1–10.5.70.254` | `10.5.71.1–10.5.71.254` |
| Broadcast address | `10.5.70.255` | `10.5.71.255` |
| Total / usable addresses | 256 / 254 | 256 / 254 |

### Proposed address allocation

**VLAN 570 — Client network**

| Address or range | Purpose | Method |
|---|---|---|
| `10.5.70.1` | Default gateway | Static |
| `10.5.70.2–10.5.70.19` | Network equipment and access-point management | Static |
| `10.5.70.20–10.5.70.49` | Printers, phones, cameras, and other fixed/shared devices | Static; DHCP reservations are an alternative if required |
| `10.5.70.50–10.5.70.199` | Employee and wireless client devices | DHCP pool (150 addresses) |
| `10.5.70.200–10.5.70.254` | Future growth and additional reservations | Reserved |

**VLAN 571 — Server network**

| Address or range | Purpose | Method |
|---|---|---|
| `10.5.71.1` | Default gateway | Static |
| `10.5.71.2–10.5.71.9` | Reserved server-network addresses | Reserved |
| `10.5.71.10` | Proposed application host / Ubuntu Server VM | Static |
| `10.5.71.11–10.5.71.12` | Other company servers | Static |
| `10.5.71.13–10.5.71.49` | Additional servers and infrastructure | Reserved for static allocation |
| `10.5.71.50–10.5.71.254` | Future server capacity | Unallocated |

## 4. Subnetting

Both networks use a `/24` prefix. IPv4 addresses contain 32 bits, leaving 8 host bits in a `/24` subnet:

`2^(32 − 24) = 2^8 = 256 total addresses`

After excluding the network and broadcast addresses:

`256 − 2 = 254 usable host addresses`

| Network | Network address | Subnet mask | Usable hosts | Why `/24` fits |
|---|---|---|---:|---|
| Client VLAN 570 | `10.5.70.0/24` | `255.255.255.0` | 254 | Supports 25 employee computers and other client-side devices, with capacity for growth. |
| Server VLAN 571 | `10.5.71.0/24` | `255.255.255.0` | 254 | Easily accommodates the three required servers and additional systems. |

## 5. Routing, firewall, and application port

VLAN 570 and VLAN 571 are separate IP subnets, so communication between them normally requires Layer 3 routing. The assignment states that the existing firewall permits the required traffic from VLAN 570 to VLAN 571 on TCP ports `8000–8100`; configuring those network firewall rules is outside the assignment’s scope.

Allowing only the ports required by an application limits unnecessary exposure. The proposed application port is **TCP `8080`**, with the planned endpoint `10.5.71.10:8080`.

## 6. Proposed network diagram

![Proposed logical network architecture for Kvist & Bjælke ApS](network_architecture_diagram.png)

*Figure 1. The proposed logical layout, showing external connectivity, routing and the existing firewall, switching, and the two required VLANs.*

## 7. Design decisions

| Design consideration | Decision |
|---|---|
| Device types need different levels of network separation, but the assignment provides two VLANs. | Keep the required client/server split; this is simple but gives less isolation to non-server devices. |
| Mobile devices and fixed systems need different addressing methods. | Use DHCP for end-user devices, static addresses for servers and infrastructure, and static addresses or reservations for fixed/shared devices. |
| The assignment does not specify default gateways. | Propose `10.5.70.1` and `10.5.71.1`; confirm them against the network design before implementation. |

## 8. Conclusion

The proposal separates clients (`10.5.70.0/24`) from servers (`10.5.71.0/24`), with 254 usable host addresses per subnet. The two-VLAN design meets the stated requirements while keeping the network simple; additional segmentation could be considered as the company grows.

## Reference

Kvist & Bjælke ApS, *CDS.infrastructure assignment brief* supplied for this project.
